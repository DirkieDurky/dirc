namespace Dirc.Compiling.CodeGen;

interface IReturnable : IOperand
{
    void Free();
}
