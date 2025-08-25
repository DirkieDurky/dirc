using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class ControlFlowFactory
{
    public IReturnable? Generate(BinaryExpressionNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label = labelGenerator.Generate(LabelType.Condition);
        string endLabel = labelGenerator.Generate(LabelType.ConditionEnd);

        IReturnable left = context.ExprFactory.Generate(node.Left, context) ?? throw new Exception("Part of if statement was not set");
        IReturnable right = context.ExprFactory.Generate(node.Right, context) ?? throw new Exception("Part of if statement was not set");
        context.CodeGen.EmitIf(node.Operation.GetComparer().GetOpposite(), left, right, label);
        left.Free();
        right.Free();

        Register resultRegister = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        // If {
        context.CodeGen.EmitMov(new NumberLiteralNode(1), resultRegister);
        // }
        context.CodeGen.EmitJump(endLabel);
        context.CodeGen.EmitLabel(label);
        // Else {
        context.CodeGen.EmitMov(new NumberLiteralNode(0), resultRegister);
        // }
        context.CodeGen.EmitLabel(endLabel);

        return new ReturnRegister(resultRegister);
    }

    public IReturnable? GenerateIfStatement(IfStatementNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label;
        string? endLabel = null;
        if (node.ElseBody == null)
        {
            label = labelGenerator.Generate(LabelType.If);
        }
        else
        {
            label = labelGenerator.Generate(LabelType.Else);
            endLabel = labelGenerator.Generate(LabelType.IfElseEnd);
        }

        if (node.Condition is BinaryExpressionNode condition && condition.Operation.IsComparer())
        {
            IReturnable left = context.ExprFactory.Generate(condition.Left, context) ?? throw new Exception("Part of if statement was not set");
            IReturnable right = context.ExprFactory.Generate(condition.Right, context) ?? throw new Exception("Part of if statement was not set");
            context.CodeGen.EmitIf(condition.Operation.GetComparer().GetOpposite(), left, right, label);
            left.Free();
            right.Free();
        }
        else
        {
            IReturnable conditionResult = context.ExprFactory.Generate(node.Condition, context) ?? throw new Exception("If condition didn't resolve to anything");

            if (conditionResult is ReturnRegister reg)
            {
                context.CodeGen.EmitIf(Comparer.IfEq, reg, new NumberLiteralNode(0), label);
            }
            else if (conditionResult is NumberLiteralNode num)
            {
                if (num.Value == "0")
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else if (conditionResult is BooleanLiteralNode boolean)
            {
                if (!boolean.Value)
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else
            {
                throw new Exception("Invalid condition result type");
            }
        }

        foreach (AstNode stmt in node.Body)
        {
            context.ExprFactory.Generate(stmt, (CodeGenContext)context.Clone());
        }

        if (endLabel != null)
        {
            context.CodeGen.EmitJump(endLabel);
        }

        context.CodeGen.EmitLabel(label);

        if (node.ElseBody != null)
        {
            foreach (AstNode stmt in node.ElseBody)
            {
                context.ExprFactory.Generate(stmt, (CodeGenContext)context.Clone());
            }
            context.CodeGen.EmitLabel(endLabel!);
        }
        return null;
    }

    public IReturnable? GenerateWhileStatement(WhileStatementNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label;
        label = labelGenerator.Generate(LabelType.While);

        context.CodeGen.EmitLabel(label);

        foreach (AstNode stmt in node.Body)
        {
            context.ExprFactory.Generate(stmt, (CodeGenContext)context.Clone());
        }

        if (node.Condition is BinaryExpressionNode condition && condition.Operation.IsComparer())
        {
            IReturnable left = context.ExprFactory.Generate(condition.Left, context) ?? throw new Exception("Part of while statement was not set");
            IReturnable right = context.ExprFactory.Generate(condition.Right, context) ?? throw new Exception("Part of while statement was not set");
            context.CodeGen.EmitIf(condition.Operation.GetComparer(), left, right, label);
            left.Free();
            right.Free();
        }
        else
        {
            IReturnable conditionResult = context.ExprFactory.Generate(node.Condition, context) ?? throw new Exception("While condition didn't resolve to anything");

            if (conditionResult is ReturnRegister reg)
            {
                context.CodeGen.EmitIf(Comparer.IfNotEq, reg, new NumberLiteralNode(0), label);
            }
            else if (conditionResult is NumberLiteralNode num)
            {
                if (num.Value != "0")
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else if (conditionResult is BooleanLiteralNode boolean)
            {
                if (boolean.Value)
                {
                    context.CodeGen.EmitJump(label);
                }
            }
            else
            {
                throw new Exception("Invalid condition result type");
            }
        }

        return null;
    }
}
