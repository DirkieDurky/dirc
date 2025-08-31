using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class CodeGenContext : ICloneable
{
    public const int StackAlignment = 1; // By how many bytes to align the stack
    public CodeGenerator CodeGen { get; }
    public Allocator Allocator { get; }
    public FunctionFactory FunctionFactory { get; }
    public ExpressionFactory ExprFactory { get; }
    public VariableFactory VarFactory { get; }
    public BinaryFactory BinaryFactory { get; }
    public IdentifierFactory IdentifierFactory { get; }
    public CallFactory CallFactory { get; }
    public ControlFlowFactory ControlFlowFactory { get; }
    public ArrayFactory ArrayFactory { get; }
    public PointerFactory PointerFactory { get; }
    public Dictionary<string, Register> SymbolTable { get; }
    public Dictionary<string, Variable> VariableTable { get; set; } = new();
    public List<string> DeclaredFunctions { get; set; } = new();
    public int NextVariableOffset { get; set; } = 0;
    public BuildOptions BuildOptions;
    public BuildContext BuildContext;

    public CodeGenContext(
        CodeGenerator codeGen,
        Allocator allocator,
        ExpressionFactory exprFactory,
        FunctionFactory funcFactory,
        VariableFactory varFactory,
        BinaryFactory binaryFactory,
        IdentifierFactory identifierFactory,
        CallFactory callFactory,
        ControlFlowFactory conditionFactory,
        ArrayFactory arrayFactory,
        PointerFactory pointerFactory,
        Dictionary<string, Register> symbolTable,
        Dictionary<string, Variable> variableTable,
        List<string> declaredFunctions,
        int nextVariableOffset,
        BuildOptions buildOptions,
        BuildContext buildContext
    )
    {
        CodeGen = codeGen;
        Allocator = allocator;
        ExprFactory = exprFactory;
        FunctionFactory = funcFactory;
        VarFactory = varFactory;
        BinaryFactory = binaryFactory;
        IdentifierFactory = identifierFactory;
        CallFactory = callFactory;
        ControlFlowFactory = conditionFactory;
        ArrayFactory = arrayFactory;
        PointerFactory = pointerFactory;
        SymbolTable = symbolTable;
        VariableTable = variableTable;
        DeclaredFunctions = declaredFunctions;
        NextVariableOffset = nextVariableOffset;
        BuildOptions = buildOptions;
        BuildContext = buildContext;
    }

    public object Clone()
    {
        CodeGenContext newContext = new(
            CodeGen,
            new(BuildOptions),
            ExprFactory,
            FunctionFactory,
            VarFactory,
            BinaryFactory,
            IdentifierFactory,
            CallFactory,
            ControlFlowFactory,
            ArrayFactory,
            PointerFactory,
            SymbolTable.ToDictionary(x => x.Key, x => x.Value),
            VariableTable.ToDictionary(x => x.Key, x => x.Value),
            DeclaredFunctions,
            NextVariableOffset,
            BuildOptions,
            BuildContext
        );
        return newContext;
    }

    public int AllocateVariable(string name)
    {
        int offset = NextVariableOffset;
        NextVariableOffset++;
        VariableTable[name] = new Variable(name, offset);
        CodeGen.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.SP,
            new NumberLiteralNode(StackAlignment),
            Allocator.Use(RegisterEnum.sp)
        );
        return offset;
    }

    public int AllocateArray(string name, int size)
    {
        int offset = NextVariableOffset + size - StackAlignment;
        NextVariableOffset += size;
        VariableTable[name] = new Variable(name, offset);

        // Allocate space for the array on the stack
        CodeGen.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.SP,
            new NumberLiteralNode(NumberLiteralType.Decimal, size.ToString()),
            Allocator.Use(RegisterEnum.sp)
        );

        return offset;
    }
}
