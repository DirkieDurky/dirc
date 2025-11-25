using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class UnaryFactory
{
    private readonly CodeGenBase _codeGenBase;

    public UnaryFactory(CodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable? Generate(UnaryOperationNode node, CodeGenContext context)
    {
        switch (node.OperationType)
        {
            case UnaryOperationType.Negate:
                {
                    switch (node.Operand)
                    {
                        case NumberLiteralNode numberLiteral:
                            return new NumberLiteralNode(numberLiteral.Type, "-" + numberLiteral.Value);
                        default:
                            throw new Exception("Invalid negate operand");
                    }
                }
            default:
                throw new Exception("Invalid unary operation type");
        }
    }
}
