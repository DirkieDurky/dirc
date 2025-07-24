using System.Diagnostics.CodeAnalysis;
using DircCompiler.CodeGen;
using DircCompiler.Parsing;

namespace DircCompiler.Semantic;

public class SemanticAnalyzer
{
    private readonly Dictionary<string, Type> _variables = new(); // name -> type
    private readonly Dictionary<string, FunctionSignature> _functions = new(); // name -> signature
    private CompilerOptions _compilerOptions;
    private CompilerContext _compilerContext;

    private static readonly HashSet<Type> ValidTypes = new() { Int.Instance, Bool.Instance };

    private readonly Dictionary<string, Type> _validTypes = new();
    private readonly Dictionary<string, Type> _validReturnTypes = new();

    public SemanticAnalyzer(CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        _compilerContext = compilerContext;
        _compilerOptions = compilerOptions;

        foreach (Type type in ValidTypes)
        {
            _validTypes.Add(type.Name, type);
        }

        _validReturnTypes = _validTypes;
        _validReturnTypes.Add(Void.Instance.Name, Void.Instance);
    }

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
                if (!_validReturnTypes.ContainsKey(func.ReturnType.TypeName))
                {
                    throw new SemanticException($"Unknown return type '{func.ReturnType.TypeName}' in function '{func.Name}'", func.ReturnType.IdentifierToken, options, context);
                }
                foreach (FunctionParameterNode param in func.Parameters)
                {
                    if (!_validTypes.ContainsKey(param.Type.TypeName))
                    {
                        throw new SemanticException($"Unknown parameter type '{param.Type.TypeName}' in function '{func.Name}'", null, options, context);
                    }
                }
                FunctionSignature signature = new FunctionSignature(_validReturnTypes[func.ReturnType.TypeName], func.Parameters);
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

    private Type? AnalyzeNode(AstNode node, Type? expectedType, CompilerOptions options, CompilerContext context)
    {
        switch (node)
        {
            case BooleanLiteralNode:
                return Bool.Instance;
            case NumberLiteralNode:
                return Int.Instance;
            case VariableDeclarationNode varDecl:
                if (!_validTypes.ContainsKey(varDecl.Type.TypeName))
                {
                    throw new SemanticException($"Unknown type '{varDecl.TypeName}' for variable '{varDecl.Name}'", varDecl.IdentifierToken, options, context);
                }
                Type varType = _validTypes[varDecl.TypeName];
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
                    Type? initType = AnalyzeNode(varDecl.Initializer, varType, options, context);
                    if (initType != null && initType != varType)
                    {
                        throw new SemanticException($"Type mismatch in initialization of '{varDecl.Name}': expected {varType.Name}, got {initType.Name}", varDecl.IdentifierToken, options, context);
                    }
                }
                return null;
            case VariableAssignmentNode varAssign:
                if (!_variables.TryGetValue(varAssign.Name, out Type? assignType))
                {
                    throw new SemanticException($"Assignment to undeclared variable '{varAssign.Name}'", varAssign.IdentifierToken, options, context);
                }
                if (varAssign.Value != null)
                {
                    Type? valueType = AnalyzeNode(varAssign.Value, assignType, options, context);
                    if (assignType != null && valueType != null && valueType != assignType)
                    {
                        throw new SemanticException($"Type mismatch in assignment to '{varAssign.Name}': expected {assignType.Name}, got {valueType.Name}", varAssign.IdentifierToken, options, context);
                    }
                }
                return assignType;
            case IdentifierNode id:
                if (!_variables.TryGetValue(id.Name, out Type? idType))
                {
                    throw new SemanticException($"Use of undeclared variable '{id.Name}'", id.IdentifierToken, options, context);
                }
                return idType;
            case ConditionNode cond:
                Type leftType = AnalyzeNode(cond.Left, null, options, context)!;
                Type rightType = AnalyzeNode(cond.Right, null, options, context)!;
                if ((leftType != Int.Instance && leftType != Bool.Instance) || (rightType != Int.Instance && rightType != Bool.Instance))
                {
                    throw new SemanticException($"Condition operands must be int or bool, got {leftType.Name} and {rightType.Name}", null, options, context);
                }
                return Bool.Instance;
            case IfStatementNode ifStmt:
                Type condType = AnalyzeNode(ifStmt.Condition, Bool.Instance, options, context)!;
                if (condType != Bool.Instance && condType != Int.Instance)
                {
                    throw new SemanticException($"If condition must be bool or int, got {condType.Name}", null, options, context);
                }
                foreach (AstNode stmt in ifStmt.Body) AnalyzeNode(stmt, null, options, context);
                if (ifStmt.ElseBody != null) foreach (AstNode stmt in ifStmt.ElseBody) AnalyzeNode(stmt, null, options, context);
                return null;
            case WhileStatementNode whileStmt:
                Type? whileCondType = AnalyzeNode(whileStmt.Condition, Bool.Instance, options, context);
                if (whileCondType != Bool.Instance && whileCondType != Int.Instance)
                {
                    string typeString = whileCondType == null ? "null" : whileCondType.Name;
                    throw new SemanticException($"While condition must be bool or int, got {typeString}", null, options, context);
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
                    if (!_validTypes.ContainsKey(sig.Parameters[i].Type.TypeName))
                    {
                        throw new SemanticException($"Unknown type '{sig.Parameters[i].Type.TypeName}' for argument '{sig.Parameters[i].Name}'", sig.Parameters[i].IdentifierToken, options, context);
                    }
                    Type? argType = AnalyzeNode(call.Arguments[i], _validTypes[sig.Parameters[i].Type.TypeName], options, context);
                    if (argType != null && argType.Name != sig.Parameters[i].Type.TypeName)
                    {
                        throw new SemanticException($"Type mismatch in argument {i + 1} of '{call.Callee}': expected {sig.Parameters[i].Type.TypeName}, got {argType.Name}", call.CalleeToken, options, context);
                    }
                }
                return sig.ReturnType;
            case FunctionDeclarationNode func:
                // New scope for parameters
                Dictionary<string, Type> oldVars = new(_variables);
                foreach (FunctionParameterNode param in func.Parameters)
                {
                    _variables[param.Name] = _validTypes[param.Type.TypeName];
                }
                foreach (AstNode stmt in func.Body)
                {
                    if (!_validReturnTypes.ContainsKey(func.ReturnType.TypeName))
                    {
                        throw new SemanticException($"Unknown return type '{func.ReturnType.TypeName}' in function '{func.Name}'", func.ReturnType.IdentifierToken, options, context);
                    }
                    AnalyzeNode(stmt, _validReturnTypes[func.ReturnType.TypeName], options, context);
                }
                _variables.Clear();
                foreach (KeyValuePair<string, Type> kv in oldVars) _variables[kv.Key] = kv.Value;
                return null;
            case ReturnStatementNode ret:
                Type? retType = AnalyzeNode(ret.ReturnValue, expectedType, options, context);
                if (expectedType != null && retType != null && retType != expectedType)
                {
                    throw new SemanticException($"Return type mismatch: expected {expectedType.Name}, got {retType.Name}", null, options, context);
                }
                return retType;
            case ArrayDeclarationNode arrayDecl:
                if (!_validTypes.ContainsKey(arrayDecl.TypeName))
                {
                    throw new SemanticException($"Unknown type '{arrayDecl.TypeName}' for array '{arrayDecl.Name}'", arrayDecl.IdentifierToken, options, context);
                }
                Type arrayType = _validTypes[arrayDecl.TypeName];
                if (_variables.ContainsKey(arrayDecl.Name))
                {
                    throw new SemanticException($"Variable '{arrayDecl.Name}' already declared", arrayDecl.IdentifierToken, options, context);
                }

                // Check that size is an integer
                Type sizeType = AnalyzeNode(arrayDecl.Size, Int.Instance, options, context)!;
                if (sizeType != Int.Instance)
                {
                    throw new SemanticException($"Array size must be an integer, got {sizeType.Name}", null, options, context);
                }

                _variables[arrayDecl.Name] = arrayType;

                if (arrayDecl.Initializer != null)
                {
                    Type? initType = AnalyzeNode(arrayDecl.Initializer, arrayType, options, context);
                    if (initType != null && initType != arrayType)
                    {
                        throw new SemanticException($"Type mismatch in array initialization of '{arrayDecl.Name}': expected {arrayType.Name}, got {initType.Name}", arrayDecl.IdentifierToken, options, context);
                    }
                }
                return null;
            case ArrayLiteralNode arrayLit:
                if (arrayLit.Elements.Count == 0)
                {
                    return Int.Instance; // Default type for empty arrays
                }

                Type firstType = AnalyzeNode(arrayLit.Elements[0], null, options, context)!;
                foreach (AstNode element in arrayLit.Elements.Skip(1))
                {
                    Type elementType = AnalyzeNode(element, firstType, options, context)!;
                    if (elementType != firstType)
                    {
                        throw new SemanticException($"All array elements must have the same type, got {firstType.Name} and {elementType.Name}", null, options, context);
                    }
                }
                return firstType;
            case ArrayAccessNode arrayAccess:
                if (!_variables.TryGetValue(arrayAccess.ArrayName, out Type? accessArrayType))
                {
                    throw new SemanticException($"Use of undeclared array '{arrayAccess.ArrayName}'", arrayAccess.ArrayToken, options, context);
                }

                // Check that index is an integer
                Type indexType = AnalyzeNode(arrayAccess.Index, Int.Instance, options, context)!;
                if (indexType != Int.Instance)
                {
                    throw new SemanticException($"Array index must be an integer, got {indexType.Name}", null, options, context);
                }

                return accessArrayType;
            case ArrayAssignmentNode arrayAssign:
                if (!_variables.TryGetValue(arrayAssign.ArrayName, out Type? assignArrayType))
                {
                    throw new SemanticException($"Assignment to undeclared array '{arrayAssign.ArrayName}'", arrayAssign.ArrayToken, options, context);
                }

                // Check that index is an integer
                Type assignIndexType = AnalyzeNode(arrayAssign.Index, Int.Instance, options, context)!;
                if (assignIndexType != Int.Instance)
                {
                    throw new SemanticException($"Array index must be an integer, got {assignIndexType.Name}", null, options, context);
                }

                // Check that value matches array type
                Type assignValueType = AnalyzeNode(arrayAssign.Value, assignArrayType, options, context)!;
                if (assignValueType != null && assignValueType != assignArrayType)
                {
                    throw new SemanticException($"Type mismatch in array assignment to '{arrayAssign.ArrayName}': expected {assignArrayType.Name}, got {assignValueType.Name}", arrayAssign.ArrayToken, options, context);
                }

                return assignArrayType;
            default:
                // For other nodes, just recurse if they have children
                return null;
        }
    }
}
