using System.Text;
using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class CodeGenBase
{
    public readonly StringBuilder Code = new();

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
        if (item is ReturnRegister reg && reg.RegisterEnum == result.RegisterEnum) return;
        if (item is ReadonlyRegister reg2 && reg2.Register == result.RegisterEnum) return;
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
            case Operation.BitshiftLeft:
                Emit($"shl{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
                break;
            case Operation.BitshiftRight:
                Emit($"shr{opSuffix} {left.AsOperand()} {right!.AsOperand()} {result}");
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

    public void EmitFunctionCall(string label, bool unresolved)
    {
        Emit($"call {(unresolved ? "@" : "")}{label} _ _");
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

    public void EmitHalt()
    {
        Emit("halt _ _ _");
    }

    public void EmitEmptyLine()
    {
        Emit("");
    }

    public void Emit(string assembly)
    {
        if (assembly == "sub|i2 fp 14 r1")
        {

        }
        Code.AppendLine(assembly);
    }

    private bool IsAssemblyReady(IOperand item)
    {
        return item is NumberLiteralNode
            || item is BooleanLiteralNode
            || item is SimpleBinaryExpressionNode;
    }
}
