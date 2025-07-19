using DircCompiler.CodeGen;
using DircCompiler.Parsing;

namespace DircCompiler.Semantic;

public class SemanticAnalyzer
{
    private readonly Dictionary<string, string> _variables = new(); // name -> type
    private readonly Dictionary<string, FunctionSignature> _functions = new(); // name -> signature

    public void Analyze(List<AstNode> nodes, CompilerOptions options, CompilerContext context)
    {
        // First pass: collect function signatures
        // Standard library
        foreach ((string name, StandardFunction funcInfo) in StandardLibrary.Functions)
        {
            _functions.Add(name, funcInfo.Signature);
        }

        // Custom functions
        foreach (AstNode node in nodes)
        {
            if (node is FunctionDeclarationNode func)
            {
                FunctionSignature signature = new FunctionSignature(func.ReturnTypeToken.Lexeme, func.Parameters);
                if (_functions.ContainsKey(func.Name))
                {
                    throw new SemanticException($"Function '{func.Name}' already declared", func.IdentifierToken, options, context);
                }

                _functions[func.Name] = signature;

                if (CompilerContext.AssemblyKeywords.ContainsKey(func.Name))
                {
                    throw new CodeGenException($"Can't declare function with name '{func.Name}'. Reserved keyword",
                        func.IdentifierToken, options, context
                    );
                }
            }
        }
        // Second pass: analyze all nodes
        foreach (AstNode node in nodes)
        {
            AnalyzeNode(node, null, options, context);
        }
    }

    private string? AnalyzeNode(AstNode node, string? expectedType, CompilerOptions options, CompilerContext context)
    {
        switch (node)
        {
            case BooleanLiteralNode:
                return "bool";
            case NumberLiteralNode:
                return "int";
            case VariableDeclarationNode varDecl:
                string varType = varDecl.TypeName;
                if (_variables.ContainsKey(varDecl.Name))
                {
                    throw new SemanticException($"Variable '{varDecl.Name}' already declared", varDecl.IdentifierToken, options, context);
                }
                else
                {
                    _variables[varDecl.Name] = varType;
                }
                if (varDecl.Initializer != null)
                {
                    string? initType = AnalyzeNode(varDecl.Initializer, varType, options, context);
                    if (initType != null && initType != varType)
                    {
                        throw new SemanticException($"Type mismatch in initialization of '{varDecl.Name}': expected {varType}, got {initType}", varDecl.IdentifierToken, options, context);
                    }
                }
                return null;
            case VariableAssignmentNode varAssign:
                if (!_variables.TryGetValue(varAssign.Name, out string? assignType))
                {
                    throw new SemanticException($"Assignment to undeclared variable '{varAssign.Name}'", varAssign.IdentifierToken, options, context);
                }
                if (varAssign.Value != null)
                {
                    string? valueType = AnalyzeNode(varAssign.Value, assignType, options, context);
                    if (assignType != null && valueType != null && valueType != assignType)
                    {
                        throw new SemanticException($"Type mismatch in assignment to '{varAssign.Name}': expected {assignType}, got {valueType}", varAssign.IdentifierToken, options, context);
                    }
                }
                return assignType;
            case IdentifierNode id:
                if (!_variables.TryGetValue(id.Name, out string? idType))
                {
                    throw new SemanticException($"Use of undeclared variable '{id.Name}'", id.IdentifierToken, options, context);
                }
                return idType;
            case ConditionNode cond:
                string? leftType = AnalyzeNode(cond.Left, null, options, context);
                string? rightType = AnalyzeNode(cond.Right, null, options, context);
                if ((leftType != "int" && leftType != "bool") || (rightType != "int" && rightType != "bool"))
                {
                    throw new SemanticException($"Condition operands must be int or bool, got {leftType} and {rightType}", null, options, context);
                }
                return "bool";
            case IfStatementNode ifStmt:
                string? condType = AnalyzeNode(ifStmt.Condition, "bool", options, context);
                if (condType != "bool" && condType != "int")
                {
                    throw new SemanticException($"If condition must be bool or int, got {condType}", null, options, context);
                }
                foreach (AstNode stmt in ifStmt.Body) AnalyzeNode(stmt, null, options, context);
                if (ifStmt.ElseBody != null) foreach (AstNode stmt in ifStmt.ElseBody) AnalyzeNode(stmt, null, options, context);
                return null;
            case WhileStatementNode whileStmt:
                string? whileCondType = AnalyzeNode(whileStmt.Condition, "bool", options, context);
                if (whileCondType != "bool" && whileCondType != "int")
                {
                    throw new SemanticException($"While condition must be bool or int, got {whileCondType}", null, options, context);
                }
                foreach (AstNode stmt in whileStmt.Body) AnalyzeNode(stmt, null, options, context);
                return null;
            case CallExpressionNode call:
                if (!_functions.TryGetValue(call.Callee, out FunctionSignature? sig))
                {
                    throw new SemanticException($"Call to undeclared function '{call.Callee}'", call.CalleeToken, options, context);
                }
                if (call.Arguments.Count != sig.Parameters.Count)
                {
                    throw new SemanticException($"Function '{call.Callee}' expects {sig.Parameters.Count} arguments, got {call.Arguments.Count}", call.CalleeToken, options, context);
                }
                for (int i = 0; i < Math.Min(call.Arguments.Count, sig.Parameters.Count); i++)
                {
                    string? argType = AnalyzeNode(call.Arguments[i], sig.Parameters[i].TypeName, options, context);
                    if (argType != null && argType != sig.Parameters[i].TypeName)
                    {
                        throw new SemanticException($"Type mismatch in argument {i + 1} of '{call.Callee}': expected {sig.Parameters[i].TypeName}, got {argType}", call.CalleeToken, options, context);
                    }
                }
                return sig.ReturnType;
            case FunctionDeclarationNode func:
                // New scope for parameters
                Dictionary<string, string> oldVars = new Dictionary<string, string>(_variables);
                foreach (FunctionParameter param in func.Parameters)
                {
                    _variables[param.Name] = param.TypeName;
                }
                foreach (AstNode stmt in func.Body)
                {
                    AnalyzeNode(stmt, func.ReturnTypeToken.Lexeme, options, context);
                }
                _variables.Clear();
                foreach (KeyValuePair<string, string> kv in oldVars) _variables[kv.Key] = kv.Value;
                return null;
            case ReturnStatementNode ret:
                string? retType = AnalyzeNode(ret.ReturnValue, expectedType, options, context);
                if (expectedType != null && retType != null && retType != expectedType)
                {
                    throw new SemanticException($"Return type mismatch: expected {expectedType}, got {retType}", null, options, context);
                }
                return retType;
            default:
                // For other nodes, just recurse if they have children
                return null;
        }
    }
}
