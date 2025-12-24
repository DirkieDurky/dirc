using System.Text;
using Dirc.Compiling;
using Dirc.Compiling.CodeGen;
using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;
using Comparer = Dirc.Compiling.Comparer;

namespace Dirc.HAL;

public class CodeGenBaseX86 : ICodeGenBase
{
    public StringBuilder Code => _code;

    private StringBuilder _code = new();

    public void EmitLabel(string name)
    {
        Emit($"{name}:");
    }

    public void EmitComment(string comment)
    {
        Emit($"# {comment}");
    }

    public void EmitMov(IOperand item, Register result)
    {
        string op = "movq";
        string line = $"{op} {AddOperandPrefix(item)}, %{result}";
        // Prevent redundant mov (e.g., mov r0 _ r0)
        if (item is ReturnRegister reg && reg.Register.RegisterBase == result.RegisterBase) return;
        if (item is ReadonlyRegister reg2 && reg2.RegisterBase == result.RegisterBase) return;
        Emit(line);
    }

    public void EmitJump(string label)
    {
        Emit($"jmp {label}");
    }

    public void EmitBinaryOperation(Operation op, IOperand left, IOperand? right, Register result)
    {
        if (right == null && op != Operation.Not)
        {
            throw new Exception($"Right operand can not be null for operation {op}");
        }

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
                if (IsAssemblyReady(left))
                {
                    Emit($"leaq {left.AsOperand()}({AddOperandPrefix(right)}), %{result}");
                }
                else if (IsAssemblyReady(right))
                {
                    Emit($"leaq {right.AsOperand()}({AddOperandPrefix(left)}), %{result}");
                }
                else
                {
                    Emit($"leaq ({AddOperandPrefix(left)}, {AddOperandPrefix(right)}), %{result}");
                }
                break;
            case Operation.Sub:
                // Filter out unnecessary statements
                if (right!.AsOperand() == "0")
                {
                    EmitMov(left!, result);
                    break;
                }
                if (right is NumberLiteralNode numberLiteralNode && int.Parse(numberLiteralNode.Value) < 0)
                {

                }
                EmitMov(left, result);
                Emit($"subq {AddOperandPrefix(right)}, %{result}");
                break;
            case Operation.And:
                EmitMov(left, result);
                Emit($"andq {AddOperandPrefix(right!)}, %{result}");
                break;
            case Operation.Or:
                EmitMov(left, result);
                Emit($"orq {AddOperandPrefix(right!)}, %{result}");
                break;
            case Operation.Not:
                EmitMov(left, result);
                Emit($"notq %{result}");
                break;
            case Operation.Xor:
                EmitMov(left, result);
                Emit($"xorq {AddOperandPrefix(right!)}, %{result}");
                break;
            case Operation.Mul:
                EmitMov(left, result);
                Emit($"imulq {AddOperandPrefix(right!)}, %{result}");
                break;
            case Operation.Div:
                Emit("push %rax");
                Emit("push %rdx");
                Emit($"movq {AddOperandPrefix(left)}, %rax");
                if (IsAssemblyReady(right!))
                {
                    Emit($"movq ${right!.AsOperand()}, %rdx");
                    Emit($"idivq %rdx");
                }
                else
                {
                    Emit($"idivq %{right!.AsOperand()}");
                }
                Emit($"movq %rax, %{result}");
                Emit("pop %rdx");
                Emit("pop %rax");
                break;
            case Operation.Mod:
                Emit("push %rax");
                Emit("push %rdx");
                Emit($"movq {AddOperandPrefix(left)}, %rax");
                if (IsAssemblyReady(right!))
                {
                    Emit($"movq ${right!.AsOperand()}, %rdx");
                    Emit($"idivq %rdx");
                }
                else
                {
                    Emit($"idivq %{right!.AsOperand()}");
                }
                Emit($"movq %rdx, %{result}");
                Emit("pop %rdx");
                Emit("pop %rax");
                break;
            case Operation.BitshiftLeft:
                EmitMov(left, result);
                Emit($"shlq {right!.AsOperand()}, {result}");
                break;
            case Operation.BitshiftRight:
                EmitMov(left, result);
                Emit($"shrq {right!.AsOperand()}, {result}");
                break;
        }
    }

    public void EmitIf(Comparer cond, IOperand left, IOperand right, string result, Allocator allocator)
    {
        if (IsAssemblyReady(left) && IsAssemblyReady(right))
        {
            int leftValue = int.Parse(left.AsOperand());
            int rightValue = int.Parse(right.AsOperand());

            bool evaluation = cond switch
            {
                Comparer.IfEq => leftValue == rightValue,
                Comparer.IfLess => leftValue < rightValue,
                Comparer.IfNotEq => leftValue != rightValue,
                Comparer.IfLessOrEq => leftValue <= rightValue,
                Comparer.IfMore => leftValue > rightValue,
                Comparer.IfMoreOrEq => leftValue >= rightValue,
                _ => throw new Exception("Invalid condition"),
            };

            if (evaluation)
            {
                Emit($"jmp {result}");
            }
            return;
        }

        if (IsAssemblyReady(left) || IsAssemblyReady(right))
        {
            long immediateValue;
            string registerValue;
            if (IsAssemblyReady(left))
            {
                immediateValue = long.Parse(left.AsOperand());
                registerValue = right.AsOperand();
            }
            else
            {
                immediateValue = long.Parse(right.AsOperand());
                registerValue = left.AsOperand();
            }

            string immediateValueStr;
            Register? tmp = null;
            if (immediateValue > int.MaxValue)
            {
                tmp = allocator.Allocate(Allocator.RegisterType.CallerSaved);
                Emit($"movabs ${immediateValue}, %{tmp}");
                immediateValueStr = $"%{tmp}";
            }
            else
            {
                immediateValueStr = $"${immediateValue}";
            }
            Emit($"cmpq {immediateValueStr}, %{registerValue}");
            tmp?.Free();
        }
        else
        {
            Emit($"cmpq %{left.AsOperand()}, %{right.AsOperand()}");
        }

        switch (cond)
        {
            case Comparer.IfEq:
                Emit($"je {result}");
                break;
            case Comparer.IfLess:
                Emit($"jne {result}");
                break;
            case Comparer.IfLessOrEq:
                Emit($"jl {result}");
                break;
            case Comparer.IfMore:
                Emit($"jle {result}");
                break;
            case Comparer.IfMoreOrEq:
                Emit($"jg {result}");
                break;
            case Comparer.IfNotEq:
                Emit($"jge {result}");
                break;
        }
    }

    public void EmitFunctionCall(string label, bool unresolved)
    {
        Emit($"call {(unresolved ? "@" : "")}{label}");
    }

    public void EmitReturn(bool final = true)
    {
        Emit($"ret");
        if (final)
        {
            EmitEmptyLine();
        }
    }

    public void EmitPush(IOperand value)
    {
        Emit($"pushq {AddOperandPrefix(value)}");
    }

    public void EmitPop(Register result)
    {
        Emit($"popq %{result}");
    }

    public void EmitStore(IOperand value, IOperand address)
    {
        string addressStr = IsAssemblyReady(address) ? $"${address.AsOperand()}" : $"(%{address.AsOperand()})";
        // string addressStr = AddOperandPrefix(address);
        Emit($"movq {AddOperandPrefix(value)}, {addressStr}");
    }

    public void EmitLoad(IOperand location, Register result)
    {
        string addressStr = IsAssemblyReady(location) ? $"${location.AsOperand()}" : $"(%{location.AsOperand()})";
        Emit($"movq {addressStr}, %{result}");
    }

    public void EmitNoop()
    {
        Emit($"nop");
    }

    public void EmitHalt()
    {
        throw new Exception("Halt not allowed on X86");
    }

    public void EmitExit()
    {
        Emit("mov $60, %rax");
        Emit("xor %rdi, %rdi");
        Emit("syscall");
        EmitEmptyLine();
    }

    public void EmitEmptyLine()
    {
        Emit("");
    }

    public void Emit(string assembly)
    {
        _code.AppendLine(assembly);
    }

    private bool IsAssemblyReady(IOperand item)
    {
        return item is NumberLiteralNode
            || item is BooleanLiteralNode
            || item is SimpleBinaryExpressionNode;
    }

    private string AddOperandPrefix(IOperand item)
    {
        return IsAssemblyReady(item) ? $"${item.AsOperand()}" : $"%{item.AsOperand()}";
    }
}
