using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class CodeGenerator
{
    public CodeGenContext Context { get; init; }
    public LabelGenerator LabelGenerator { get; init; } = new();

    private readonly List<string> _code = new();

    public CodeGenerator(CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        Allocator allocator = new(compilerOptions);

        FunctionFactory funcFactory = new FunctionFactory(compilerOptions);
        ExpressionFactory exprFactory = new ExpressionFactory(compilerOptions, LabelGenerator);
        VariableFactory varFactory = new VariableFactory();
        BinaryFactory binaryFactory = new BinaryFactory();
        IdentifierFactory identifierFactory = new IdentifierFactory();
        CallFactory callFactory = new CallFactory();
        ControlFlowFactory conditionFactory = new ControlFlowFactory();
        ArrayFactory arrayFactory = new ArrayFactory();
        PointerFactory pointerFactory = new PointerFactory();
        Register screenPtr = allocator.Allocate(Allocator.RegisterType.CalleeSaved);
        Context = new CodeGenContext(
            this,
            allocator,
            exprFactory,
            funcFactory,
            varFactory,
            binaryFactory,
            identifierFactory,
            callFactory,
            conditionFactory,
            arrayFactory,
            pointerFactory,
            [],
            [],
            0,
            compilerOptions,
            compilerContext,
            screenPtr
        );
    }

    public string[] Generate(List<AstNode> nodes)
    {
        // Set the stack pointer to the end of the RAM to grow downwards
        // In other compilers the OS does this but since we don't have an OS we'll do it ourselves
        Context.CodeGen.EmitMov(
            new NumberLiteralNode(CompilerEnvironment.MaxRamValue),
            Context.Allocator.Use(RegisterEnum.sp)
        );
        // Initialize the screen buffer
        Context.CodeGen.EmitMov(new NumberLiteralNode(CompilerEnvironment.ScreenBufferStart), Context.ScreenPtr);

        EmitJump("_start");
        EmitEmptyLine();

        // Compile calls to runtime or standard library functions or an imported library
        List<string> compiledFunctions = new();

        foreach (CallExpressionNode callNode in nodes.Where(node => node is CallExpressionNode))
        {
            if (RuntimeLibrary.HasFunction(callNode.Callee) && !compiledFunctions.Contains(callNode.Callee))
            {
                RuntimeLibrary.CompileFunction(callNode.Callee, Context);
                compiledFunctions.Add(callNode.Callee);
            }
            else
            {
                string stdLibPath = Path.Combine(AppContext.BaseDirectory, "lib", "stdlib");
                List<string> libraryFiles = Directory.EnumerateFiles(stdLibPath, "*.dirc", SearchOption.AllDirectories).ToList();

                foreach (string file in libraryFiles)
                {
                    Console.WriteLine(file);
                }
            }
        }

        // Compile functions before rest of the code so they're at the top
        foreach (FunctionDeclarationNode funcNode in nodes.Where(node => node is FunctionDeclarationNode))
        {
            Context.FunctionFactory.Generate(funcNode, (CodeGenContext)Context.Clone());
        }

        EmitLabel("_start");

        Context.CodeGen.EmitMov(ReadonlyRegister.SP, Context.Allocator.Use(RegisterEnum.fp));

        foreach (AstNode node in nodes)
        {
            switch (node)
            {
                case ImportStatementNode:
                case FunctionDeclarationNode:
                    break;
                default:
                    Context.ExprFactory.Generate(node, Context);
                    break;
            }
        }
        Context.ScreenPtr.Free();
        return _code.ToArray();
    }

    public void EmitLabel(string name)
    {
        Emit($"label {name}");
    }

    public void EmitComment(string comment)
    {
        Emit($"# {comment}");
    }

    public void EmitMov(IOperand item, Register result)
    {
        string op = "mov" + (IsAssemblyReady(item) ? "|i1" : "");
        string line = $"{op} {item.AsOperand()} _ {result}";
        // Prevent redundant mov (e.g., mov r0 _ r0)
        if (item is Register reg && reg.RegisterEnum == result.RegisterEnum) return;
        Emit(line);
    }

    public void EmitJump(string label)
    {
        Emit($"jump {label} _ pc");
    }

    public void EmitBinaryOperation(Operation op, IOperand left, IOperand? right, Register result)
    {
        if (right == null && op != Operation.Not)
        {
            throw new Exception($"Right operand can not be null for operation {op}");
        }

        string opSuffix = "";
        if (IsAssemblyReady(left)) opSuffix += "|i1";
        if (right != null && IsAssemblyReady(right)) opSuffix += "|i2";

        switch (op)
        {
            case Operation.Add:
                // Filter out unnecessary statements
                if (left.AsOperand() == "0")
                {
                    EmitMov(right!, result);
                    break;
                }
                else if (right!.AsOperand() == "0")
                {
                    EmitMov(left!, result);
                    break;
                }
                Emit($"add{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.Sub:
                // Filter out unnecessary statements
                if (left.AsOperand() == "0")
                {
                    EmitMov(right!, result);
                    break;
                }
                else if (right!.AsOperand() == "0")
                {
                    EmitMov(left!, result);
                    break;
                }
                Emit($"sub{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.And:
                Emit($"and{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.Or:
                Emit($"or{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.Not:
                Emit($"not{opSuffix} {left.AsOperand()} _ {result}");
                break;
            case Operation.Xor:
                Emit($"xor{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.Mul:
                Emit($"mul{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.Div:
                Emit($"div{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.Mod:
                Emit($"mod{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
        }
    }

    public void EmitIf(Comparer cond, IOperand left, IOperand right, string result)
    {
        string opSuffix = "";
        if (IsAssemblyReady(left)) opSuffix += "|i1";
        if (IsAssemblyReady(right)) opSuffix += "|i2";

        switch (cond)
        {
            case Comparer.IfEq:
                Emit($"ifEq{opSuffix} {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Comparer.IfLess:
                Emit($"ifLess{opSuffix} {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Comparer.IfLessOrEq:
                Emit($"ifLessOrEq{opSuffix} {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Comparer.IfMore:
                Emit($"ifMore{opSuffix} {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Comparer.IfMoreOrEq:
                Emit($"ifMoreOrEq{opSuffix} {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Comparer.IfNotEq:
                Emit($"ifNotEq{opSuffix} {left.AsOperand()} {right.AsOperand()} {result}");
                break;
        }
    }

    public void EmitFunctionCall(string label)
    {
        Emit($"call {label} _ _");
    }

    public void EmitReturn(bool final = true)
    {
        Emit($"return _ _ _");
        if (final)
        {
            EmitEmptyLine();
        }
    }

    public void EmitPush(IOperand value)
    {
        string opSuffix = "";
        if (IsAssemblyReady(value)) opSuffix += "|i1";

        Emit($"push{opSuffix} {value.AsOperand()} _ _");
    }

    public void EmitPop(Register result)
    {
        Emit($"pop _ _ {result}");
    }

    public void EmitStore(IOperand value, IOperand address)
    {
        string opSuffix = "";
        if (IsAssemblyReady(value)) opSuffix += "|i1";
        if (IsAssemblyReady(address)) opSuffix += "|i2";

        Emit($"store{opSuffix} {value.AsOperand()} {address.AsOperand()} _");
    }

    public void EmitLoad(IOperand location, Register result)
    {
        string opSuffix = "";
        if (IsAssemblyReady(location)) opSuffix += "|i1";

        Emit($"load{opSuffix} {location.AsOperand()} _ {result}");
    }

    public void EmitNoop()
    {
        Emit($"noop _ _ _");
    }

    public void EmitEmptyLine()
    {
        Emit("");
    }

    public void Emit(string assembly)
    {
        _code.Add(assembly);
    }

    private bool IsAssemblyReady(IOperand item)
    {
        return item is NumberLiteralNode
            || item is BooleanLiteralNode
            || item is SimpleBinaryExpressionNode;
    }
}
