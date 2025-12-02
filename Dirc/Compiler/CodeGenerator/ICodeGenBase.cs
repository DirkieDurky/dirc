using System.Text;
using Dirc.Compiling.CodeGen.Allocating;

namespace Dirc.Compiling.CodeGen;

public interface ICodeGenBase
{
    public StringBuilder Code { get; }
    void EmitStartLabel();
    void EmitLabel(string name);
    void EmitComment(string comment);
    void EmitMov(IOperand item, Register result);
    void EmitJump(string label);
    void EmitBinaryOperation(Operation op, IOperand left, IOperand? right, Register result);
    void EmitIf(Comparer cond, IOperand left, IOperand right, string result);
    void EmitFunctionCall(string label, bool unresolved);
    void EmitReturn(bool final = true);
    void EmitPush(IOperand value);
    void EmitPop(Register result);
    void EmitStore(IOperand value, IOperand address);
    void EmitLoad(IOperand location, Register result);
    void EmitNoop();
    void EmitHalt();
    void EmitEmptyLine();
    void Emit(string assembly);
}
