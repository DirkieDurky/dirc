using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class BinaryFactory
{
    private readonly CodeGenBase _codeGenBase;

    public BinaryFactory(CodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable? Generate(BinaryExpressionNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        if (node.Operation.IsComparer())
        {
            return context.ControlFlowFactory.GenerateCondition(node, context, labelGenerator);
        }
        IReturnable leftOperand = context.ExprFactory.Generate(node.Left, context) ?? throw new Exception("Left operand of binary expression is missing");
        IReturnable rightOperand = context.ExprFactory.Generate(node.Right, context) ?? throw new Exception("Right operand of binary expression is missing");

        if (leftOperand is NumberLiteralNode && rightOperand is NumberLiteralNode)
        {
            return new SimpleBinaryExpressionNode(node);
        }

        Operation op = node.Operation;
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        _codeGenBase.EmitBinaryOperation(op, leftOperand, rightOperand, result);
        leftOperand.Free();
        rightOperand.Free();

        return new ReturnRegister(result);
    }
}
