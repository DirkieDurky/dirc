using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;
using Dirc.HAL;

namespace Dirc.Compiling.CodeGen;

class UnaryFactory
{
    private readonly ICodeGenBase _codeGenBase;

    public UnaryFactory(ICodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable? Generate(UnaryOperationNode node, CodeGenContext context)
    {
        switch (node.OperationType)
        {
            case UnaryOperationType.Negate:
                {
                    if (node.Operand is NumberLiteralNode numberLiteral)
                        return new NumberLiteralNode(numberLiteral.Type, "-" + numberLiteral.Value);

                    IReturnable operand = context.ExprFactory.Generate(node.Operand, context) ?? throw new Exception("Negate operand couldn't generate");
                    Register reg = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
                    context.CodeGenBase.EmitBinaryOperation(Operation.Sub, new NumberLiteralNode(0), operand, reg);
                    return new ReturnRegister(reg);
                }
            default:
                throw new Exception("Invalid unary operation type");
        }
    }
}
