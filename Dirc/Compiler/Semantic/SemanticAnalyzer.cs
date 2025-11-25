using System.Text.Json;
using Dirc.Compiling.CodeGen;
using Dirc.Compiling.Lexing;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.Semantic;

public class SemanticAnalyzer
{
    private readonly Dictionary<string, Type> _variables = new(); // name -> type
    private readonly Dictionary<string, FunctionSignature> _functions = new(); // name -> signature
    private Options _options;
    private BuildContext _buildContext;

    private static readonly HashSet<SimpleType> ValidTypes = new() { Int.Instance, Bool.Instance, Char.Instance };

    private readonly Dictionary<string, SimpleType> _validTypes = new();
    private readonly Dictionary<string, SimpleType> _validReturnTypes = new();

    public SemanticAnalyzer(Options options, BuildContext buildContext)
    {
        _buildContext = buildContext;
        _options = options;

        foreach (SimpleType type in ValidTypes)
        {
            _validTypes.Add(type.Name, type);
        }

        _validReturnTypes = _validTypes;
        _validReturnTypes.Add(Void.Instance.Name, Void.Instance);
    }

    public List<AstNode> Analyze(List<AstNode> nodes, SymbolTable symbolTable)
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
        foreach (MetaFile.Function function in symbolTable.Functions)
        {
            SimpleType returnType = TypeFromString(function.ReturnType, true);

            List<FunctionParameter> functionParameters = [];
            foreach (MetaFile.Param param in function.Parameters)
            {
                SimpleType paramType = TypeFromString(param.Type, false);
                functionParameters.Add(new(paramType, param.Name));
            }

            if (_functions.ContainsKey(function.Name))
            {
                // Ignore the error and overwrite original function if we're compiling stdlib itself
                if (_options.IgnoreStdlib && stdMetaFile.Functions.Any(f => f.Name == function.Name))
                {
                    _functions.Remove(function.Name);
                }
                else
                {
                    throw new SemanticException($"Function '{function.Name}' already declared", null, _options, _buildContext);
                }
            }

            if (BuildEnvironment.AssemblyKeywords.ContainsKey(function.Name))
            {
                throw new CodeGenException($"Can't declare function with name '{function.Name}'. Reserved keyword",
                    null, _options, _buildContext
                );
            }

            _functions.Add(function.Name, new FunctionSignature(returnType, functionParameters));
        }

        if (_options.IgnoreSemanticErrors)
        {
            try
            {
                AnalyzeNodes(nodes);
            }
            catch (SemanticException ex)
            {
                Console.WriteLine("Semantic exception ignored: " + ex.Message);
            }
        }
        else
        {
            AnalyzeNodes(nodes);
        }

        return nodes;
    }

    private void AnalyzeNodes(List<AstNode> nodes)
    {
        // Second pass: analyze all nodes
        foreach (AstNode node in nodes)
        {
            AnalyzeNode(node, null, _options, _buildContext);
        }
    }

    private SimpleType? AnalyzeNode(AstNode node, SimpleType? expectedType, Options options, BuildContext context)
    {
        switch (node)
        {
            case BooleanLiteralNode:
                {
                    return Bool.Instance;
                }
            case NumberLiteralNode:
                {
                    return Int.Instance;
                }
            case CharNode:
                {
                    return Char.Instance;
                }
            case StringLiteralNode:
                {
                    return null;
                }
            case VariableDeclarationNode varDecl:
                {
                    Type varComplexType = ResolveType(varDecl.Type);
                    SimpleType varType = varComplexType.SimpleType;
                    if (_variables.ContainsKey(varDecl.Name))
                    {
                        throw new SemanticException($"Variable '{varDecl.Name}' already declared", varDecl.IdentifierToken, options, context);
                    }
                    else
                    {
                        _variables[varDecl.Name] = varComplexType;
                    }
                    if (varDecl.Initializer != null)
                    {
                        SimpleType? initType = AnalyzeNode(varDecl.Initializer, varType, options, context);
                        if (initType != null && initType != varType)
                        {
                            // Allow int assigned to pointer for now
                            if (varType is Pointer && initType == Int.Instance) return null;
                            // Allow anything for void pointers
                            if (initType is Pointer initTypePtr && initTypePtr.BaseType == Void.Instance) return null;

                            // Since we currently don't have typecasts yet, just allow when both types are primitives
                            if (initType is PrimitiveType && varType is PrimitiveType) return null;

                            throw new SemanticException($"Type mismatch in initialization of '{varDecl.Name}': expected {varType.Name}, got {initType.Name}", varDecl.IdentifierToken, options, context);
                        }
                    }
                    return null;
                }
            case VariableAssignmentNode varAssign:
                {
                    Type? assignType = null;
                    if (varAssign.Name != null && !_variables.TryGetValue(varAssign.Name, out assignType))
                    {
                        throw new SemanticException($"Assignment to undeclared variable '{varAssign.Name}'", varAssign.TargetName, options, context);
                    }
                    SimpleType? simpleAssignType = assignType == null ? null : assignType.SimpleType;
                    if (varAssign.Value != null)
                    {
                        SimpleType? valueType = AnalyzeNode(varAssign.Value, simpleAssignType, options, context);
                        if (simpleAssignType != null && valueType != null && valueType != simpleAssignType)
                        {
                            // Allow int assigned to pointer for now
                            if (!(simpleAssignType is Pointer && valueType == Int.Instance))
                            {
                                // Since we currently don't have typecasts yet, just allow when both types are primitives
                                if (simpleAssignType is PrimitiveType && valueType is PrimitiveType) return simpleAssignType;

                                throw new SemanticException($"Type mismatch in assignment to '{varAssign.Name}': expected {simpleAssignType.Name}, got {valueType.Name}", varAssign.TargetName, options, context);
                            }
                        }
                    }
                    return simpleAssignType;
                }
            case IdentifierNode id:
                {
                    if (!_variables.TryGetValue(id.Name, out Type? idType))
                    {
                        throw new SemanticException($"Use of undeclared variable '{id.Name}'", id.IdentifierToken, options, context);
                    }
                    return idType.SimpleType;
                }
            case BinaryExpressionNode binNode:
                {
                    SimpleType? returnTypeOverride = null;

                    bool supportPointerOperands = binNode.Operation == Operation.Add || binNode.Operation == Operation.Sub;

                    SimpleType leftType = AnalyzeNode(binNode.Left, null, options, context)!;
                    if (supportPointerOperands && leftType is Pointer leftPtr)
                    {
                        leftType = leftPtr.BaseType;
                        returnTypeOverride = leftPtr;
                    }
                    SimpleType rightType = AnalyzeNode(binNode.Right, null, options, context)!;
                    if (supportPointerOperands && rightType is Pointer rightPtr)
                    {
                        rightType = rightPtr.BaseType;
                        if (returnTypeOverride != null) throw new SemanticException($"Can't do pointer arithmetic with 2 pointers. One argument should be of type int", null, options, context);
                        returnTypeOverride = rightPtr;
                    }

                    if (!(leftType is PrimitiveType or Void && rightType is PrimitiveType or Void || leftType == rightType))
                    {
                        throw new SemanticException($"Condition operands must be a primitive type, got {leftType.Name} and {rightType.Name}", null, options, context);
                    }
                    return returnTypeOverride ?? Helpers.ReturnTypes[binNode.Operation];
                }
            case IfStatementNode ifStmt:
                {
                    SimpleType condType = AnalyzeNode(ifStmt.Condition, Bool.Instance, options, context)!;
                    if (condType != Bool.Instance && condType != Int.Instance)
                    {
                        throw new SemanticException($"If condition must be bool or int, got {condType.Name}", null, options, context);
                    }
                    foreach (AstNode stmt in ifStmt.Body) AnalyzeNode(stmt, null, options, context);
                    if (ifStmt.ElseBody != null) foreach (AstNode stmt in ifStmt.ElseBody) AnalyzeNode(stmt, null, options, context);
                    return null;
                }
            case WhileStatementNode whileStmt:
                {
                    SimpleType? whileCondType = AnalyzeNode(whileStmt.Condition, Bool.Instance, options, context);
                    if (whileCondType != Bool.Instance && whileCondType != Int.Instance)
                    {
                        string typeString = whileCondType == null ? "null" : whileCondType.Name;
                        throw new SemanticException($"While condition must be bool or int, got {typeString}", null, options, context);
                    }
                    foreach (AstNode stmt in whileStmt.Body) AnalyzeNode(stmt, null, options, context);
                    return null;
                }
            case CallExpressionNode call:
                {
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
                        SimpleType parameterType = sig.Parameters[i].Type;
                        SimpleType? argType = AnalyzeNode(call.Arguments[i], parameterType, options, context);
                        if (argType != null && argType != parameterType)
                        {
                            // Allow anything for void pointers
                            if (parameterType is Pointer paramTypePtr && paramTypePtr.BaseType == Void.Instance) continue;

                            // Since we currently don't have typecasts yet, just allow when both types are primitives
                            if (parameterType is PrimitiveType && argType is PrimitiveType) continue;

                            throw new SemanticException($"Type mismatch in argument {i + 1} of '{call.Callee}': expected {sig.Parameters[i].Type.Name}, got {argType.Name}", call.CalleeToken, options, context);
                        }
                    }
                    return sig.ReturnType;
                }
            case FunctionDeclarationNode func:
                {
                    if (func.ReturnType.IsArray)
                    {
                        throw new SemanticException("Functions may not return arrays. Please allocate an array on the heap and return it's pointer instead.", func.IdentifierToken, options, context);
                    }
                    // New scope for parameters
                    Dictionary<string, Type> oldVars = new(_variables);
                    foreach (FunctionParameterNode param in func.Parameters)
                    {
                        if (param.Type is PointerTypeNode pointerType)
                        {
                            _variables[param.Name] = new Type(Pointer.Of(_validTypes[pointerType.BaseType.Name]), param.Type.ArraySizes);
                        }
                        else
                        {
                            _variables[param.Name] = new Type(_validTypes[param.Type.Name], param.Type.ArraySizes);
                        }
                    }
                    foreach (AstNode stmt in func.Body)
                    {
                        SimpleType returnType = TypeFromString(func.ReturnType.Name, true);
                        AnalyzeNode(stmt, returnType, options, context);
                    }
                    _variables.Clear();
                    foreach (KeyValuePair<string, Type> kv in oldVars) _variables[kv.Key] = kv.Value;
                    return null;
                }
            case ReturnStatementNode ret:
                {
                    SimpleType? retType = AnalyzeNode(ret.ReturnValue, expectedType, options, context);
                    if (expectedType != null && retType != null && retType != expectedType)
                    {
                        // When expected type is void*, allow any pointer for return type
                        if (expectedType is Pointer expPtr && expPtr.BaseType is Void && retType is Pointer) return retType;

                        if (retType is Void && expectedType is PrimitiveType) return retType;

                        throw new SemanticException($"Return type mismatch: expected {expectedType.Name}, got {retType.Name}", null, options, context);
                    }
                    return retType;
                }
            case ArrayDeclarationNode arrayDecl:
                {
                    List<int?> foundSizes = [];

                    // Find array sizes (in all dimensions) based on initializer
                    switch (arrayDecl.Initializer)
                    {
                        case ArrayLiteralNode:
                            {
                                AstNode currentArrayLiteral = arrayDecl.Initializer;
                                while (currentArrayLiteral is ArrayLiteralNode arrayLiteral)
                                {
                                    int size = arrayLiteral.Elements.Count;
                                    foundSizes.Add(size);

                                    // Check if sizes of subArrays match with each other
                                    int? firstSize = null;
                                    if (arrayLiteral.Elements[0] is ArrayLiteralNode subArrayLiteral && subArrayLiteral.Elements.Count > 1)
                                    {
                                        firstSize = ((ArrayLiteralNode)arrayLiteral.Elements[0]).Elements.Count;
                                        for (int i = 1; i < arrayLiteral.Elements.Count; i++)
                                        {
                                            size = ((ArrayLiteralNode)arrayLiteral.Elements[i]).Elements.Count;
                                            if (firstSize != size)
                                            {
                                                throw new SemanticException($"Sizes of arrays in array literal don't match", null, options, context);
                                            }
                                        }
                                    }
                                    else if (arrayLiteral.Elements[0] is StringLiteralNode)
                                    {
                                        foundSizes.Add(null);
                                        break;
                                    }
                                    currentArrayLiteral = arrayLiteral.Elements[0];
                                }
                                break;
                            }
                        case IdentifierNode identifierNode:
                            foundSizes.Add(_variables[identifierNode.Name].ArraySizes[0]);
                            break;
                        case ArrayAccessNode arrayAccessNode:
                            {
                                // Get the array at the root
                                ArrayAccessNode currentArrayAccess = arrayAccessNode;
                                int depth = 1;

                                while (currentArrayAccess.Array is ArrayAccessNode subArrayAccess)
                                {
                                    currentArrayAccess = subArrayAccess;
                                    depth++;
                                }

                                if (currentArrayAccess.Array is not IdentifierNode identifierNode)
                                {
                                    throw new Exception("Array access array was of invalid type");
                                }

                                foundSizes.Add(_variables[identifierNode.Name].ArraySizes[depth]);
                                break;
                            }
                    }

                    // Fill missing sizes if possible
                    if (arrayDecl.Sizes.Count == 0)
                    {
                        if (foundSizes.Count == 0)
                        {
                            throw new SemanticException($"Can't find out array length of '{arrayDecl.Name}' implicitly. Please specify array length.", arrayDecl.IdentifierToken, options, context);
                        }
                        arrayDecl.Sizes.AddRange(foundSizes);
                    }
                    else
                    {
                        if (arrayDecl.Sizes.Except(foundSizes).Count() > 0)
                        {
                            throw new SemanticException(
                                $"Given array size(s) don't match initalizer. Given: {string.Join("", arrayDecl.Sizes.Select(x => $"[{x}]"))}. Initializer: {string.Join("", foundSizes.Select(x => $"[{x}]"))}",
                            arrayDecl.IdentifierToken, options, context);
                        }
                    }

                    string trimmedTypeName = arrayDecl.TypeName.TrimEnd('*');
                    if (!_validTypes.ContainsKey(trimmedTypeName))
                    {
                        throw new SemanticException($"Unknown type '{trimmedTypeName}' for array '{arrayDecl.Name}'", arrayDecl.IdentifierToken, options, context);
                    }
                    if (_variables.ContainsKey(arrayDecl.Name))
                    {
                        throw new SemanticException($"Variable '{arrayDecl.Name}' already declared", arrayDecl.IdentifierToken, options, context);
                    }

                    SimpleType arrayType = _validTypes[trimmedTypeName];
                    for (int i = 0; i < arrayDecl.Type.ArraySizes.Count; i++)
                    {
                        arrayType = Pointer.Of(arrayType);
                    }
                    _variables[arrayDecl.Name] = new Type(arrayType, arrayDecl.Sizes); // Array variables return a pointer to their first element

                    if (arrayDecl.Initializer != null)
                    {
                        SimpleType? initType = AnalyzeNode(arrayDecl.Initializer, arrayType, options, context);
                        if (initType != null && initType.Name != arrayType.Name) // Ignore differences in pointer type for now
                        {
                            throw new SemanticException($"Type mismatch in array initialization of '{arrayDecl.Name}': expected {arrayType.Name}, got {initType.Name}", arrayDecl.IdentifierToken, options, context);
                        }
                    }
                    return null;
                }
            case ArrayLiteralNode arrayLiteral:
                {
                    if (arrayLiteral.Elements.Count == 0)
                    {
                        return Int.Instance; // Default type for empty arrays
                    }

                    SimpleType firstType = AnalyzeNode(arrayLiteral.Elements[0], null, options, context)!;
                    foreach (AstNode element in arrayLiteral.Elements.Skip(1))
                    {
                        SimpleType elementType = AnalyzeNode(element, firstType, options, context)!;
                        if (elementType != firstType)
                        {
                            throw new SemanticException($"All array elements must have the same type, got {firstType.Name} and {elementType.Name}", null, options, context);
                        }
                    }

                    ArrayLiteralNode currentNode = arrayLiteral;
                    SimpleType? finalType = firstType == null ? null : Pointer.Of(firstType);
                    while (currentNode.Elements[0] is ArrayLiteralNode subArrayLiteral)
                    {
                        finalType = firstType == null ? null : Pointer.Of(firstType);
                        currentNode = subArrayLiteral;
                    }

                    return finalType;
                }
            case ArrayAccessNode arrayAccess:
                {
                    // Get array at the root
                    ArrayAccessNode currentArrayAccess = arrayAccess;

                    while (currentArrayAccess.Array is ArrayAccessNode subArrayAccess)
                    {
                        currentArrayAccess = subArrayAccess;
                    }

                    if (currentArrayAccess.Array is not IdentifierNode arrayIdentifier)
                    {
                        throw new Exception("Array access array was of invalid type");
                    }

                    SimpleType arrayType = _variables[arrayIdentifier.Name].SimpleType;

                    if (arrayType is not Pointer)
                    {
                        throw new SemanticException($"Trying to index on variable of type {arrayType.Name}", null, options, context);
                    }

                    return AnalyzeArrayAccess(arrayAccess, arrayType, options, context);
                }
            case ArrayAssignmentNode arrayAssign:
                {
                    if (arrayAssign.Array is ArrayAccessNode) return AnalyzeNode(arrayAssign.Array, null, options, context);
                    IdentifierNode arrayIdentifier = (IdentifierNode)arrayAssign.Array;

                    if (!_variables.TryGetValue(arrayIdentifier.Name, out Type? assignArrayType))
                    {
                        throw new SemanticException($"Assignment to undeclared array '{arrayIdentifier.Name}'", arrayIdentifier.IdentifierToken, options, context);
                    }

                    // Check that index is an integer
                    SimpleType assignIndexType = AnalyzeNode(arrayAssign.Index, Int.Instance, options, context)!;
                    if (assignIndexType != Int.Instance)
                    {
                        throw new SemanticException($"Array index must be an integer, got {assignIndexType.Name}", null, options, context);
                    }

                    SimpleType valueType = ((Pointer)assignArrayType.SimpleType).BaseType;
                    // Check that value matches array type
                    SimpleType assignValueType = AnalyzeNode(arrayAssign.Value, valueType, options, context)!;

                    if (assignValueType != valueType)
                    {
                        // Since we currently don't have typecasts yet, just allow when both types are primitives
                        if (assignValueType is PrimitiveType && valueType is PrimitiveType) return valueType;

                        throw new SemanticException(
                            $"Type mismatch in array assignment to '{arrayIdentifier.Name}': expected {valueType.Name}, got {assignValueType.Name}", arrayIdentifier.IdentifierToken, options, context);
                    }

                    return assignArrayType.SimpleType;
                }
            case PointerDereferenceNode deref:
                {
                    SimpleType ptrType = AnalyzeNode(deref.PointerExpression, null, options, context)!;
                    if (ptrType is Pointer p) return p.BaseType;
                    throw new SemanticException($"Cannot dereference non-pointer type {ptrType.Name}", null, options, context);
                }
            case AddressOfNode addr:
                {
                    SimpleType varType2 = AnalyzeNode(addr.Variable, null, options, context)!;
                    return Pointer.Of(varType2);
                }
            case ArrLenNode arrLenNode:
                {
                    var array = _variables[arrLenNode.Array.Name];
                    if (arrLenNode.Dimension >= array.ArraySizes.Count)
                    {
                        throw new SemanticException($"Array {arrLenNode.Array.Name} doesn't have {arrLenNode.Dimension} dimensions", null, options, context);
                    }
                    int? length = array.ArraySizes[arrLenNode.Dimension] ?? throw new SemanticException(
                        $"Array {arrLenNode.Array.Name} has {arrLenNode.Dimension} dimensions but doesn't have a fixed length for "
                        + $"{arrLenNode.Dimension}th dimension. It's likely an array of strings. Check the length of a string using "
                        + $"the 'strlen()' function.", null, options, context);
                    arrLenNode.ComputedLength = (int)length;
                    return Int.Instance;
                }
            default:
                return null;
        }
    }

    private SimpleType AnalyzeArrayAccess(ArrayAccessNode arrayAccess, SimpleType arrayType, Options options, BuildContext context)
    {
        if (arrayType is not Pointer)
        {
            throw new SemanticException($"Trying to index on variable of type {arrayType.Name}", null, options, context);
        }

        SimpleType arrayValuesType = ((Pointer)arrayType).BaseType;
        SimpleType finalType = arrayValuesType;
        if (arrayAccess.Array is ArrayAccessNode subArrayAccess)
        {
            finalType = AnalyzeArrayAccess(subArrayAccess, arrayValuesType, options, context);
        }

        // Check that index is an integer
        SimpleType indexType = AnalyzeNode(arrayAccess.Index, Int.Instance, options, context)!;
        if (indexType != Int.Instance)
        {
            throw new SemanticException($"Array index must be an integer, got {indexType.Name}", null, options, context);
        }

        return finalType;
    }

    private SimpleType ResolveSimpleType(TypeNode node)
    {
        if (node is NamedTypeNode named)
        {
            if (_validTypes.TryGetValue(named.Name, out var t)) return t;
            throw new SemanticException($"Unknown type '{named.Name}'", named.IdentifierToken, _options, _buildContext);
        }
        if (node is PointerTypeNode ptr)
        {
            return Pointer.Of(ResolveSimpleType(ptr.BaseType));
        }
        throw new SemanticException($"Unknown type node", null, _options, _buildContext);
    }

    private Type ResolveType(TypeNode node)
    {
        return new Type(ResolveSimpleType(node), node.ArraySizes);
    }

    private SimpleType TypeFromString(string type, bool isReturnType)
    {
        if (type.EndsWith('*'))
        {
            return Pointer.Of(TypeFromString(type[..^1], isReturnType));
        }

        if (isReturnType)
        {
            if (_validReturnTypes.TryGetValue(type, out SimpleType? t)) return t;
        }
        else
        {
            if (_validTypes.TryGetValue(type, out SimpleType? t)) return t;
        }
        throw new SemanticException($"Unknown type '{type}'", new Token(TokenType.Identifier, type, null, -1), _options, _buildContext);
    }
}
