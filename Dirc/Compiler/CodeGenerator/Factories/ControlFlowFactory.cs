using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;
using Dirc.HAL;

namespace Dirc.Compiling.CodeGen;

class ControlFlowFactory
{
    private readonly ICodeGenBase _codeGenBase;

    public ControlFlowFactory(ICodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public IReturnable? GenerateCondition(BinaryExpressionNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label = labelGenerator.Generate(LabelType.Condition, context);
        string endLabel = labelGenerator.Generate(LabelType.ConditionEnd, context);

        IReturnable left = context.ExprFactory.Generate(node.Left, context) ?? throw new Exception("Part of if statement was not set");
        IReturnable right = context.ExprFactory.Generate(node.Right, context) ?? throw new Exception("Part of if statement was not set");
        _codeGenBase.EmitIf(node.Operation.GetComparer().GetOpposite(), left, right, label);
        left.Free();
        right.Free();

        Register resultRegister = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);
        // If {
        _codeGenBase.EmitMov(new NumberLiteralNode(1), resultRegister);
        // }
        _codeGenBase.EmitJump(endLabel);
        _codeGenBase.EmitLabel(label);
        // Else {
        _codeGenBase.EmitMov(new NumberLiteralNode(0), resultRegister);
        // }
        _codeGenBase.EmitLabel(endLabel);

        return new ReturnRegister(resultRegister);
    }

    public IReturnable? GenerateIfStatement(IfStatementNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string label;
        string? endLabel = null;
        if (node.ElseBody == null)
        {
            label = labelGenerator.Generate(LabelType.If, context);
        }
        else
        {
            label = labelGenerator.Generate(LabelType.Else, context);
            endLabel = labelGenerator.Generate(LabelType.IfElseEnd, context);
        }

        if (node.Condition is BinaryExpressionNode condition && condition.Operation.IsComparer())
        {
            IReturnable left = context.ExprFactory.Generate(condition.Left, context) ?? throw new Exception("Part of if statement was not set");
            IReturnable right = context.ExprFactory.Generate(condition.Right, context) ?? throw new Exception("Part of if statement was not set");
            _codeGenBase.EmitIf(condition.Operation.GetComparer().GetOpposite(), left, right, label);
            left.Free();
            right.Free();
        }
        else
        {
            IReturnable conditionResult = context.ExprFactory.Generate(node.Condition, context) ?? throw new Exception("If condition didn't resolve to anything");

            if (conditionResult is ReturnRegister reg)
            {
                _codeGenBase.EmitIf(Comparer.IfEq, reg, new NumberLiteralNode(0), label);
            }
            else if (conditionResult is NumberLiteralNode num)
            {
                if (num.Value == "0")
                {
                    _codeGenBase.EmitJump(label);
                }
            }
            else if (conditionResult is BooleanLiteralNode boolean)
            {
                if (!boolean.Value)
                {
                    _codeGenBase.EmitJump(label);
                }
            }
            else
            {
                throw new Exception("Invalid condition result type");
            }
            conditionResult.Free();
        }

        GenerateBody(node.Body, context);

        if (endLabel != null)
        {
            _codeGenBase.EmitJump(endLabel);
        }

        _codeGenBase.EmitLabel(label);

        if (node.ElseBody != null)
        {
            GenerateBody(node.ElseBody, context);
            _codeGenBase.EmitLabel(endLabel!);
        }
        return null;
    }

    public IReturnable? GenerateWhileStatement(WhileStatementNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        string loopLabel = labelGenerator.Generate(LabelType.While, context);
        string loopEndLabel = labelGenerator.Generate(LabelType.WhileEnd, context);

        context.LoopLabelStack.Push((loopLabel, loopEndLabel));

        _codeGenBase.EmitLabel(loopLabel);

        if (node.Condition is BinaryExpressionNode condition && condition.Operation.IsComparer())
        {
            IReturnable left = context.ExprFactory.Generate(condition.Left, context) ?? throw new Exception("Part of while statement was not set");
            IReturnable right = context.ExprFactory.Generate(condition.Right, context) ?? throw new Exception("Part of while statement was not set");
            _codeGenBase.EmitIf(condition.Operation.GetComparer().GetOpposite(), left, right, loopEndLabel);
            left.Free();
            right.Free();
        }
        else
        {
            IReturnable conditionResult = context.ExprFactory.Generate(node.Condition, context) ?? throw new Exception("While condition didn't resolve to anything");

            if (conditionResult is ReturnRegister reg)
            {
                _codeGenBase.EmitIf(Comparer.IfEq, reg, new NumberLiteralNode(0), loopEndLabel);
            }
            else if (conditionResult is NumberLiteralNode num)
            {
                if (num.Value == "0")
                {
                    _codeGenBase.EmitJump(loopEndLabel);
                }
            }
            else if (conditionResult is BooleanLiteralNode boolean)
            {
                if (!boolean.Value)
                {
                    _codeGenBase.EmitJump(loopEndLabel);
                }
            }
            else
            {
                throw new Exception("Invalid condition result type");
            }
        }

        GenerateBody(node.Body, context);

        _codeGenBase.EmitJump(loopLabel);

        _codeGenBase.EmitLabel(loopEndLabel);

        context.LoopLabelStack.Pop();

        return null;
    }

    public IReturnable? GenerateForStatement(ForStatementNode node, CodeGenContext context, LabelGenerator labelGenerator)
    {
        // Generate initialization
        if (node.Initialization != null)
        {
            IReturnable? initResult = context.ExprFactory.Generate(node.Initialization, context);
            initResult?.Free();
        }

        // Generate loop similar to while statement
        string loopLabel = labelGenerator.Generate(LabelType.While, context);
        string loopEndLabel = labelGenerator.Generate(LabelType.WhileEnd, context);
        string incrementLabel = labelGenerator.Generate(LabelType.While, context);

        // For loops, continue jumps to the increment section
        context.LoopLabelStack.Push((incrementLabel, loopEndLabel));

        _codeGenBase.EmitLabel(loopLabel);

        // Generate condition
        if (node.Condition != null)
        {
            if (node.Condition is BinaryExpressionNode condition && condition.Operation.IsComparer())
            {
                IReturnable left = context.ExprFactory.Generate(condition.Left, context) ?? throw new Exception("Part of for condition was not set");
                IReturnable right = context.ExprFactory.Generate(condition.Right, context) ?? throw new Exception("Part of for condition was not set");
                _codeGenBase.EmitIf(condition.Operation.GetComparer().GetOpposite(), left, right, loopEndLabel);
                left.Free();
                right.Free();
            }
            else
            {
                IReturnable conditionResult = context.ExprFactory.Generate(node.Condition, context) ?? throw new Exception("For condition didn't resolve to anything");

                if (conditionResult is ReturnRegister reg)
                {
                    _codeGenBase.EmitIf(Comparer.IfEq, reg, new NumberLiteralNode(0), loopEndLabel);
                }
                else if (conditionResult is NumberLiteralNode num)
                {
                    if (num.Value == "0")
                    {
                        _codeGenBase.EmitJump(loopEndLabel);
                    }
                }
                else if (conditionResult is BooleanLiteralNode boolean)
                {
                    if (!boolean.Value)
                    {
                        _codeGenBase.EmitJump(loopEndLabel);
                    }
                }
                else
                {
                    throw new Exception("Invalid condition result type");
                }
                conditionResult.Free();
            }
        }

        GenerateBody(node.Body, context);

        _codeGenBase.EmitLabel(incrementLabel);

        if (node.Increment != null)
        {
            IReturnable? incrResult = context.ExprFactory.Generate(node.Increment, context);
            incrResult?.Free();
        }

        _codeGenBase.EmitJump(loopLabel);

        _codeGenBase.EmitLabel(loopEndLabel);

        context.LoopLabelStack.Pop();

        return null;
    }

    public void GenerateBody(List<AstNode> body, CodeGenContext context)
    {
        CodeGenContext scopeSpecificContext = (CodeGenContext)context.Clone();
        foreach (AstNode stmt in body)
        {
            IReturnable? result = context.ExprFactory.Generate(stmt, scopeSpecificContext);
            result?.Free();
        }
    }

    public IReturnable? GenerateBreakStatement(BreakNode node, CodeGenContext context)
    {
        if (context.LoopLabelStack.Count == 0)
        {
            throw new Exception("break statement outside of loop");
        }

        (_, string breakLabel) = context.LoopLabelStack.Peek();
        _codeGenBase.EmitJump(breakLabel);
        return null;
    }

    public IReturnable? GenerateContinueStatement(ContinueNode node, CodeGenContext context)
    {
        if (context.LoopLabelStack.Count == 0)
        {
            throw new Exception("continue statement outside of loop");
        }

        (string continueLabel, _) = context.LoopLabelStack.Peek();
        _codeGenBase.EmitJump(continueLabel);
        return null;
    }
}
