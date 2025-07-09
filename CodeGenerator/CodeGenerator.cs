class CodeGenerator
{
    private List<string> code = [];

    public string[] Generate(List<AstNode> nodes)
    {
        Allocator allocator = new Allocator([]);
        ExpressionCodeFactory exprFactory = new ExpressionCodeFactory(allocator);
        FunctionCodeFactory funcFactory = new FunctionCodeFactory(allocator, exprFactory);

        foreach (var node in nodes)
        {
            switch (node)
            {
                case FunctionDeclarationNode func:
                    funcFactory.Generate(func);
                    break;
                default:
                    exprFactory.Generate(node);
                    break;
            }
        }
        return code.ToArray();
    }

    public void EmitLabel(string name)
    {
        Emit($"label {name}:");
    }

    public void EmitComment(string comment)
    {
        Emit($"# {comment}");
    }

    public void EmitMov(IOperand item, Register result)
    {
        string op = "mov" + (item is NumberLiteralNode ? "|i1" : "");
        Emit($"{op} {item} _ {result}");
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
                Emit($"add{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.Sub:
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
    }

    public void EmitPush(IOperand value)
    {
        string opSuffix = "";
        if (value is NumberLiteralNode) opSuffix += "|i1";

        Emit($"push{opSuffix} {value} _ _");
    }

    public void EmitPop(Register result)
    {
        Emit($"pop _ _ {result}");
    }

    public void EmitStore(IOperand value, IOperand location)
    {
        string opSuffix = "";
        if (value is NumberLiteralNode) opSuffix += "|i1";
        if (location is NumberLiteralNode) opSuffix += "|i2";

        Emit($"store{opSuffix} {value} {location} _");
    }

    public void EmitLoad(IOperand location, Register register)
    {
        string opSuffix = "";
        if (location is NumberLiteralNode) opSuffix += "|i1";

        Emit($"load{opSuffix} {location} _ {register}");
    }

    public void EmitNoop()
    {
        Emit($"noop _ _ _");
    }

    public void Emit(string assembly)
    {
        code.Add(assembly);
    }
}
