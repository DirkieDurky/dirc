using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class CodeGenContext : ICloneable
{
    public CodeGenerator CodeGen { get; }
    public CodeGenBase CodeGenBase { get; }
    public Allocator Allocator { get; }
    public FunctionFactory FunctionFactory { get; }
    public ExpressionFactory ExprFactory { get; }
    public VariableFactory VarFactory { get; }
    public BinaryFactory BinaryFactory { get; }
    public IdentifierFactory IdentifierFactory { get; }
    public CallFactory CallFactory { get; }
    public ControlFlowFactory ControlFlowFactory { get; }
    public ArrayFactory ArrayFactory { get; }
    public StringFactory StringFactory { get; }
    public PointerFactory PointerFactory { get; }
    public Dictionary<string, Register> RegisterTable { get; }
    public Dictionary<string, Variable> VariableTable { get; set; } = new();
    public List<string> DeclaredFunctions { get; set; } = new();
    public int NextVariableOffset { get; set; } = 0;
    public Options Options;
    public BuildContext BuildContext;

    public CodeGenContext(
        CodeGenerator codeGen,
        CodeGenBase codeGenBase,
        Allocator allocator,
        ExpressionFactory exprFactory,
        FunctionFactory funcFactory,
        VariableFactory varFactory,
        BinaryFactory binaryFactory,
        IdentifierFactory identifierFactory,
        CallFactory callFactory,
        ControlFlowFactory conditionFactory,
        ArrayFactory arrayFactory,
        StringFactory stringFactory,
        PointerFactory pointerFactory,
        Dictionary<string, Register> symbolTable,
        Dictionary<string, Variable> variableTable,
        List<string> declaredFunctions,
        int nextVariableOffset,
        Options options,
        BuildContext buildContext
    )
    {
        CodeGen = codeGen;
        CodeGenBase = codeGenBase;
        Allocator = allocator;
        ExprFactory = exprFactory;
        FunctionFactory = funcFactory;
        VarFactory = varFactory;
        BinaryFactory = binaryFactory;
        IdentifierFactory = identifierFactory;
        CallFactory = callFactory;
        ControlFlowFactory = conditionFactory;
        ArrayFactory = arrayFactory;
        StringFactory = stringFactory;
        PointerFactory = pointerFactory;
        RegisterTable = symbolTable;
        VariableTable = variableTable;
        DeclaredFunctions = declaredFunctions;
        NextVariableOffset = nextVariableOffset;
        Options = options;
        BuildContext = buildContext;
    }

    public object CloneAndResetAllocator()
    {
        CodeGenContext newContext = new(
            CodeGen,
            CodeGenBase,
            new(Options),
            ExprFactory,
            FunctionFactory,
            VarFactory,
            BinaryFactory,
            IdentifierFactory,
            CallFactory,
            ControlFlowFactory,
            ArrayFactory,
            StringFactory,
            PointerFactory,
            RegisterTable.ToDictionary(x => x.Key, x => x.Value),
            VariableTable.ToDictionary(x => x.Key, x => x.Value),
            DeclaredFunctions,
            NextVariableOffset,
            Options,
            BuildContext
        );
        return newContext;
    }

    public object Clone()
    {
        CodeGenContext newContext = new(
            CodeGen,
            CodeGenBase,
            Allocator,
            ExprFactory,
            FunctionFactory,
            VarFactory,
            BinaryFactory,
            IdentifierFactory,
            CallFactory,
            ControlFlowFactory,
            ArrayFactory,
            StringFactory,
            PointerFactory,
            RegisterTable.ToDictionary(x => x.Key, x => x.Value),
            VariableTable.ToDictionary(x => x.Key, x => x.Value),
            DeclaredFunctions,
            NextVariableOffset,
            Options,
            BuildContext
        );
        return newContext;
    }

    public int AllocateVariable(string name)
    {
        int offset = NextVariableOffset;
        NextVariableOffset++;
        VariableTable[name] = new Variable(name, offset);
        CodeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.SP,
            new NumberLiteralNode(BuildEnvironment.StackAlignment),
            Allocator.Use(RegisterEnum.sp)
        );
        return offset;
    }

    public int AllocateArray(int size, string? name)
    {
        int offset = NextVariableOffset + size - BuildEnvironment.StackAlignment;
        NextVariableOffset += size;
        if (name != null)
        {
            VariableTable[name] = new Variable(name, offset, true);
        }

        // Allocate space for the array on the stack
        CodeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.SP,
            new NumberLiteralNode(NumberLiteralType.Decimal, size.ToString()),
            Allocator.Use(RegisterEnum.sp)
        );

        return offset;
    }

    public void AssignNameToArray(int offset, string name)
    {
        VariableTable[name] = new Variable(name, offset, true);
    }
}
