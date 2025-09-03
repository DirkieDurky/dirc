using System.Text.Json;
using System.Text.Json.Serialization;
using Dirc.Compiling.CodeGen;
using Dirc.Compiling.Lexing;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.Semantic;

public class SemanticAnalyzer
{
    private readonly Dictionary<string, Type> _variables = new(); // name -> type
    private readonly Dictionary<string, FunctionSignature> _functions = new(); // name -> signature
    private BuildOptions _buildOptions;
    private BuildContext _buildContext;

    private static readonly HashSet<Type> ValidTypes = new() { Int.Instance, Bool.Instance, Char.Instance };

    private readonly Dictionary<string, Type> _validTypes = new();
    private readonly Dictionary<string, Type> _validReturnTypes = new();

    public SemanticAnalyzer(BuildOptions buildOptions, BuildContext buildContext)
    {
        _buildContext = buildContext;
        _buildOptions = buildOptions;

        foreach (Type type in ValidTypes)
        {
            _validTypes.Add(type.Name, type);
        }

        _validReturnTypes = _validTypes;
        _validReturnTypes.Add(Void.Instance.Name, Void.Instance);
    }

    public void Analyze(List<AstNode> nodes, SymbolTable symbolTable, BuildOptions options, BuildContext context)
    {
        // First pass: collect function signatures

        // Runtime library
        foreach ((string name, FunctionSignature signature) in RuntimeLibrary.GetAllFunctionSignatures())
        {
            _functions.Add(name, signature);
        }

        // Standard library
        string stdLibMetaPath = Path.Combine(AppContext.BaseDirectory, "lib", "stdlib", "stdlib.meta");
        MetaFile.Root stdMetaFile = JsonSerializer.Deserialize<MetaFile.Root>(File.ReadAllText(stdLibMetaPath)) ?? throw new Exception("Standard Library could not be read");
        foreach (MetaFile.Function stdFunc in stdMetaFile.Functions)
        {
            List<FunctionParameter> funcParams = new();
            foreach (MetaFile.Param param in stdFunc.Parameters)
            {
                funcParams.Add(new FunctionParameter(TypeFromString(param.Type, false), param.Name));
            }
            _functions.Add(stdFunc.Name, new FunctionSignature(TypeFromString(stdFunc.ReturnType, true), funcParams));
        }

        // Imported libraries
        foreach (ImportStatementNode importNode in nodes.Where(n => n is ImportStatementNode))
        {
            string libraryName = importNode.LibraryName;
            if (libraryName == "runtime") continue;

            string libMetaPath = Path.Combine(AppContext.BaseDirectory, "lib", libraryName, libraryName + ".meta");
            string libMetaText = File.ReadAllText(libMetaPath);

            MetaFile.Root libMetaFile = JsonSerializer.Deserialize<MetaFile.Root>(libMetaText) ?? throw new Exception($"Library '{libraryName}' could not be read");
            foreach (MetaFile.Function libFunc in libMetaFile.Functions)
            {
                List<FunctionParameter> funcParams = new();
                foreach (MetaFile.Param param in libFunc.Parameters)
                {
                    funcParams.Add(new FunctionParameter(TypeFromString(param.Type, false), param.Name));
                }
                _functions.Add(libFunc.Name, new FunctionSignature(TypeFromString(libFunc.ReturnType, true), funcParams));
            }
        }

        // From the symbol table
        foreach (MetaFile.Function function in symbolTable.FunctionTable)
        {
            Type returnType = TypeFromString(function.ReturnType, true);

            List<FunctionParameter> functionParameters = [];
            foreach (MetaFile.Param param in function.Parameters)
            {
                Type paramType = TypeFromString(param.Type, false);
                functionParameters.Add(new(paramType, param.Name));
            }

            if (_functions.ContainsKey(function.Name))
            {
                throw new SemanticException($"Function '{function.Name}' already declared", null, options, context);
            }

            if (BuildEnvironment.AssemblyKeywords.ContainsKey(function.Name))
            {
                throw new CodeGenException($"Can't declare function with name '{function.Name}'. Reserved keyword",
                    null, options, context
                );
            }

            _functions.Add(function.Name, new FunctionSignature(returnType, functionParameters));
        }

        // Second pass: analyze all nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            AstNode node = nodes[i];
            AnalyzeNode(node, null, options, context);
        }
    }

    private Type? AnalyzeNode(AstNode node, Type? expectedType, BuildOptions options, BuildContext context)
    {
        switch (node)
        {
            case BooleanLiteralNode:
                return Bool.Instance;
            case NumberLiteralNode:
                return Int.Instance;
            case VariableDeclarationNode varDecl:
                Type varType = ResolveType(varDecl.Type);
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
                        // Allow int assigned to pointer for now
                        if (varType is Pointer && initType == Int.Instance) return null;
                        // Allow anything for void pointers
                        if (initType is Pointer initTypePtr && initTypePtr.BaseType == Void.Instance) return null;

                        throw new SemanticException($"Type mismatch in initialization of '{varDecl.Name}': expected {varType.Name}, got {initType.Name}", varDecl.IdentifierToken, options, context);
                    }
                }
                return null;
            case VariableAssignmentNode varAssign:
                Type? assignType = null;
                if (varAssign.Name != null && !_variables.TryGetValue(varAssign.Name, out assignType))
                {
                    throw new SemanticException($"Assignment to undeclared variable '{varAssign.Name}'", varAssign.TargetName, options, context);
                }
                if (varAssign.Value != null)
                {
                    Type? valueType = AnalyzeNode(varAssign.Value, assignType, options, context);
                    if (assignType != null && valueType != null && valueType != assignType)
                    {
                        // Allow int assigned to pointer for now
                        if (!(assignType is Pointer && valueType == Int.Instance))
                        {
                            throw new SemanticException($"Type mismatch in assignment to '{varAssign.Name}': expected {assignType.Name}, got {valueType.Name}", varAssign.TargetName, options, context);
                        }
                    }
                }
                return assignType;
            case IdentifierNode id:
                if (!_variables.TryGetValue(id.Name, out Type? idType))
                {
                    throw new SemanticException($"Use of undeclared variable '{id.Name}'", id.IdentifierToken, options, context);
                }
                return idType;
            case BinaryExpressionNode binNode:
                Type leftType = AnalyzeNode(binNode.Left, null, options, context)!;
                Type rightType = AnalyzeNode(binNode.Right, null, options, context)!;
                if (!(leftType is PrimitiveType && rightType is PrimitiveType || leftType == rightType))
                {
                    throw new SemanticException($"Condition operands must be int or bool, got {leftType.Name} and {rightType.Name}", null, options, context);
                }
                return Helpers.ReturnTypes[binNode.Operation];
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
                    Type parameterType = sig.Parameters[i].Type;
                    Type? argType = AnalyzeNode(call.Arguments[i], parameterType, options, context);
                    if (argType != null && argType != parameterType)
                    {
                        // Allow anything for void pointers
                        if (parameterType is Pointer paramTypePtr && paramTypePtr.BaseType == Void.Instance) return sig.ReturnType;

                        throw new SemanticException($"Type mismatch in argument {i + 1} of '{call.Callee}': expected {sig.Parameters[i].Type.Name}, got {argType.Name}", call.CalleeToken, options, context);
                    }
                }
                return sig.ReturnType;
            case FunctionDeclarationNode func:
                // New scope for parameters
                Dictionary<string, Type> oldVars = new(_variables);
                foreach (FunctionParameterNode param in func.Parameters)
                {
                    if (param.Type is PointerTypeNode pointerType)
                    {
                        _variables[param.Name] = Pointer.Of(_validTypes[pointerType.BaseType.Name]);
                    }
                    else
                    {
                        _variables[param.Name] = _validTypes[param.Type.Name];
                    }
                }
                foreach (AstNode stmt in func.Body)
                {
                    Type returnType = TypeFromString(func.ReturnType.Name, true);
                    AnalyzeNode(stmt, returnType, options, context);
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

                _variables[arrayDecl.Name] = Pointer.Of(arrayType); // Array variables return a pointer to their first element

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

                if (accessArrayType is Pointer ptr)
                {
                    accessArrayType = ptr.BaseType;
                    arrayAccess.ArrayIsPointer = true;
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

                if (assignArrayType is Pointer assignPtr)
                {
                    assignArrayType = assignPtr.BaseType;
                    arrayAssign.ArrayIsPointer = true;
                }

                if (assignValueType != null && assignValueType != assignArrayType)
                {
                    throw new SemanticException($"Type mismatch in array assignment to '{arrayAssign.ArrayName}': expected {assignArrayType.Name}, got {assignValueType.Name}", arrayAssign.ArrayToken, options, context);
                }

                return assignArrayType;
            case PointerDereferenceNode deref:
                Type ptrType = AnalyzeNode(deref.PointerExpression, null, options, context)!;
                if (ptrType is Pointer p) return p.BaseType;
                throw new SemanticException($"Cannot dereference non-pointer type {ptrType.Name}", null, options, context);
            case AddressOfNode addr:
                Type varType2 = AnalyzeNode(addr.Variable, null, options, context)!;
                return Pointer.Of(varType2);
            default:
                return null;
        }
    }

    private Type ResolveType(TypeNode node)
    {
        if (node is NamedTypeNode named)
        {
            if (_validTypes.TryGetValue(named.Name, out var t)) return t;
            throw new SemanticException($"Unknown type '{named.Name}'", named.IdentifierToken, _buildOptions, _buildContext);
        }
        if (node is PointerTypeNode ptr)
        {
            return Pointer.Of(ResolveType(ptr.BaseType));
        }
        throw new SemanticException($"Unknown type node", null, _buildOptions, _buildContext);
    }

    private Type TypeFromString(string type, bool isReturnType)
    {
        if (type.EndsWith('*'))
        {
            return Pointer.Of(TypeFromString(type[..^1], isReturnType));
        }

        if (isReturnType)
        {
            if (_validReturnTypes.TryGetValue(type, out Type? t)) return t;
        }
        else
        {
            if (_validTypes.TryGetValue(type, out Type? t)) return t;
        }
        throw new SemanticException($"Unknown type '{type}'", new Token(TokenType.Identifier, type, null, -1), _buildOptions, _buildContext);
    }
}
