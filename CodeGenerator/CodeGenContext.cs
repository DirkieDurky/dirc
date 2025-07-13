using Dirc.CodeGen.Allocating;
using Dirc.Parsing;

namespace Dirc.CodeGen;

class CodeGenContext : ICloneable
{
    public const int StackAlignment = 1; // By how many bytes to align the stack
    public CodeGenerator CodeGen { get; }
    public Allocator Allocator { get; }
    public ExpressionCodeFactory ExprFactory { get; }
    public FunctionCodeFactory FuncFactory { get; }
    public Dictionary<string, Register> SymbolTable { get; }
    public Dictionary<string, Variable> VariableTable { get; set; } = new();
    public FunctionTable FunctionTable { get; } = new();
    public int NextVariableOffset { get; set; } = 0;

    public CodeGenContext(
        CodeGenerator codeGen,
        Allocator allocator,
        ExpressionCodeFactory exprFactory,
        FunctionCodeFactory funcFactory,
        Dictionary<string, Register> symbolTable,
        Dictionary<string, Variable> variableTable,
        FunctionTable functionTable,
        int nextVariableOffset
    )
    {
        CodeGen = codeGen;
        Allocator = allocator;
        ExprFactory = exprFactory;
        FuncFactory = funcFactory;
        SymbolTable = symbolTable;
        VariableTable = variableTable;
        FunctionTable = functionTable;
        NextVariableOffset = nextVariableOffset;
    }

    public object Clone()
    {
        CodeGenContext newContext = new(
            CodeGen,
            Allocator,
            ExprFactory,
            FuncFactory,
            SymbolTable.ToDictionary(x => x.Key, x => x.Value),
            VariableTable.ToDictionary(x => x.Key, x => x.Value),
            (FunctionTable)FunctionTable.Clone(),
            NextVariableOffset
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
}
