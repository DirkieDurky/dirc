using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class CodeGenContext : ICloneable
{
    public const int StackAlignment = 1; // By how many bytes to align the stack
    public CodeGenerator CodeGen { get; }
    public Allocator Allocator { get; }
    public ExpressionCodeFactory ExprFactory { get; }
    public FunctionCodeFactory FuncFactory { get; }
    public Dictionary<string, Register> SymbolTable { get; }
    public Dictionary<string, Variable> VariableTable { get; set; } = new();
    public int NextVariableOffset { get; set; } = 0;
    public CompilerOptions CompilerOptions;
    public CompilerContext CompilerContext;

    public CodeGenContext(
        CodeGenerator codeGen,
        Allocator allocator,
        ExpressionCodeFactory exprFactory,
        FunctionCodeFactory funcFactory,
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
        FuncFactory = funcFactory;
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
            FuncFactory,
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
