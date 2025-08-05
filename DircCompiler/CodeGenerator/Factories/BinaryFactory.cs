using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

internal class BinaryFactory
{
    public IReturnable? Generate(BinaryExpressionNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        if (node.Operation.IsComparer())
        {
            return context.ConditionFactory.Generate(node, context, labelGenerator);
        }
        IReturnable leftOperand = context.ExprFactory.Generate(node.Left, context) ?? throw new Exception("Left operand of binary expression is missing");
        IReturnable rightOperand = context.ExprFactory.Generate(node.Right, context) ?? throw new Exception("Right operand of binary expression is missing");

        if (leftOperand is NumberLiteralNode && rightOperand is NumberLiteralNode)
        {
            return new SimpleBinaryExpressionNode(node);
        }

        Operation op = node.Operation;
        Register result = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        context.CodeGen.EmitBinaryOperation(op, leftOperand, rightOperand, result);
        leftOperand.Free();
        rightOperand.Free();

        return new ReturnRegister(result);
    }
}
