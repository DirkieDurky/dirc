using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

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
    public ConditionFactory ConditionFactory { get; }
    public ArrayFactory ArrayFactory { get; }
    public PointerFactory PointerFactory { get; }
    public Dictionary<string, Register> SymbolTable { get; }
    public Dictionary<string, Variable> VariableTable { get; set; } = new();
    public int NextVariableOffset { get; set; } = 0;
    public CompilerOptions CompilerOptions;
    public CompilerContext CompilerContext;

    public CodeGenContext(
        CodeGenerator codeGen,
        Allocator allocator,
        ExpressionFactory exprFactory,
        FunctionFactory funcFactory,
        VariableFactory varFactory,
        BinaryFactory binaryFactory,
        IdentifierFactory identifierFactory,
        CallFactory callFactory,
        ConditionFactory conditionFactory,
        ArrayFactory arrayFactory,
        PointerFactory pointerFactory,
        Dictionary<string, Register> symbolTable,
        Dictionary<string, Variable> variableTable,
        int nextVariableOffset,
        CompilerOptions compilerOptions,
        CompilerContext compilerContext
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
        ConditionFactory = conditionFactory;
        ArrayFactory = arrayFactory;
        PointerFactory = pointerFactory;
        SymbolTable = symbolTable;
        VariableTable = variableTable;
        NextVariableOffset = nextVariableOffset;
        CompilerOptions = compilerOptions;
        CompilerContext = compilerContext;
    }

    public object Clone()
    {
        CodeGenContext newContext = new(
            CodeGen,
            new(CompilerOptions),
            ExprFactory,
            FunctionFactory,
            VarFactory,
            BinaryFactory,
            IdentifierFactory,
            CallFactory,
            ConditionFactory,
            ArrayFactory,
            PointerFactory,
            SymbolTable.ToDictionary(x => x.Key, x => x.Value),
            VariableTable.ToDictionary(x => x.Key, x => x.Value),
            NextVariableOffset,
            CompilerOptions,
            CompilerContext
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
