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
    public Dictionary<string, Variable> VariableTable { get; set; } = new();
    public List<string> DeclaredFunctions { get; set; } = new();
    // The offset to put the next variable on the stack
    public int NextVariableOffset { get; set; } = 0;
    // How many words the stack pointer is from the start
    public int StackPointerOffset { get; set; } = 0;
    public int StackframeSize { get; set; } = 0;
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
        Dictionary<string, Variable> variableTable,
        List<string> declaredFunctions,
        int nextVariableOffset,
        int stackPointerOffset,
        int stackframeSize,
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
        VariableTable = variableTable;
        DeclaredFunctions = declaredFunctions;
        NextVariableOffset = nextVariableOffset;
        StackPointerOffset = stackPointerOffset;
        StackframeSize = stackframeSize;
        Options = options;
        BuildContext = buildContext;
    }

    public object GetSubcontext()
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
            VariableTable.ToDictionary(x => x.Key, x => x.Value),
            DeclaredFunctions,
            NextVariableOffset,
            0,
            StackframeSize,
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
            (Allocator)Allocator.Clone(),
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
            VariableTable.ToDictionary(x => x.Key, x => x.Value),
            DeclaredFunctions,
            NextVariableOffset,
            StackPointerOffset,
            StackframeSize,
            Options,
            BuildContext
        );
        return newContext;
    }

    public int AllocateStackVariable(string name)
    {
        int offset = NextVariableOffset;
        NextVariableOffset++;
        VariableTable[name] = new StackStoredVariable(name, offset);
        return offset;
    }

    public int Push(IOperand operand, CodeGenContext context)
    {
        int offset = context.StackframeSize - StackPointerOffset;
        StackPointerOffset++;

        CodeGenBase.EmitPush(operand);
        return offset;
    }

    public int AllocateStackArray(int size, string? name)
    {
        int offset = NextVariableOffset + size - BuildEnvironment.StackAlignment;
        NextVariableOffset += size;
        if (name != null)
        {
            VariableTable[name] = new StackStoredVariable(name, offset, true);
        }

        return offset;
    }

    public void AssignNameToArray(int offset, string name)
    {
        VariableTable[name] = new StackStoredVariable(name, offset, true);
    }
}
