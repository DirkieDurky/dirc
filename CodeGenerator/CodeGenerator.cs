class CodeGenerator
{
    private readonly List<string> _code = new();
    public CodeGenContext Context { get; private set; }

    public CodeGenerator()
    {
        Allocator allocator = new Allocator();
        ExpressionCodeFactory exprFactory = new ExpressionCodeFactory();
        FunctionCodeFactory funcFactory = new FunctionCodeFactory();
        Context = new CodeGenContext(
            this,
            allocator,
            exprFactory,
            funcFactory,
            new(),
            new(),
            new(),
            0
            );
    }

    public string[] Generate(List<AstNode> nodes)
    {
        // Set the stack pointer to the end of the RAM to grow downwards
        // In other compilers the OS does this but since we don't have an OS we'll do it ourselves
        Context.CodeGen.EmitBinaryOperation(Operation.Sub, Register.SP, new NumberLiteralNode(CodeGenContext.StackAlignment), Register.SP);

        EmitJump("start");
        EmitEmptyLine();

        // Compile standard library
        StandardLibrary std = new StandardLibrary();
        std.Compile(Context);

        // Compile functions first
        foreach (FunctionDeclarationNode funcNode in nodes.Where(node => node is FunctionDeclarationNode))
        {
            Context.FuncFactory.Generate(funcNode, Context);
        }

        EmitLabel("start");
        Context.CodeGen.EmitMov(Register.SP, Register.FP);
        foreach (AstNode node in nodes)
        {
            switch (node)
            {
                case FunctionDeclarationNode func:
                    break;
                default:
                    Context.ExprFactory.Generate(node, Context);
                    break;
            }
        }
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
        string op = "mov" + (item is NumberLiteralNode || item is SimpleBinaryExpressionNode ? "|i1" : "");
        string line = $"{op} {item.AsOperand()} _ {result.AsOperand()}";
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
        if (left is NumberLiteralNode) opSuffix += "|i1";
        if (right is NumberLiteralNode) opSuffix += "|i2";

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
                Emit($"add{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result.AsOperand()}");
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
                Emit($"sub{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result.AsOperand()}");
                break;
            case Operation.And:
                Emit($"and{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result.AsOperand()}");
                break;
            case Operation.Or:
                Emit($"or{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result.AsOperand()}");
                break;
            case Operation.Not:
                Emit($"not{opSuffix} {left.AsOperand()} _ {result.AsOperand()}");
                break;
            case Operation.Xor:
                Emit($"xor{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result.AsOperand()}");
                break;
        }
    }

    public void EmitCondition(Condition cond, IOperand left, IOperand right, string result)
    {
        switch (cond)
        {
            case Condition.IfEq:
                Emit($"ifEq {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Condition.IfLess:
                Emit($"ifLess {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Condition.IfLessOrEq:
                Emit($"ifLessOrEq {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Condition.IfMore:
                Emit($"ifMore {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Condition.IfMoreOrEq:
                Emit($"ifMoreOrEq {left.AsOperand()} {right.AsOperand()} {result}");
                break;
            case Condition.IfNotEq:
                Emit($"ifNotEq {left.AsOperand()} {right.AsOperand()} {result}");
                break;
        }
    }

    public void EmitFunctionCall(string label)
    {
        Emit($"call {label} _ _");
    }

    public void EmitReturn()
    {
        Emit($"return _ _ _");
        EmitEmptyLine();
    }

    public void EmitPush(IOperand value)
    {
        string opSuffix = "";
        if (value is NumberLiteralNode) opSuffix += "|i1";

        Emit($"push{opSuffix} {value.AsOperand()} _ _");
    }

    public void EmitPop(Register result)
    {
        Emit($"pop _ _ {result.AsOperand()}");
    }

    public void EmitStore(IOperand value, IOperand location)
    {
        string opSuffix = "";
        if (value is NumberLiteralNode) opSuffix += "|i1";
        if (location is NumberLiteralNode) opSuffix += "|i2";

        Emit($"store{opSuffix} {value.AsOperand()} {location.AsOperand()} _");
    }

    public void EmitLoad(IOperand location, Register result)
    {
        string opSuffix = "";
        if (location is NumberLiteralNode) opSuffix += "|i1";

        Emit($"load{opSuffix} {location.AsOperand()} _ {result.AsOperand()}");
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
}
