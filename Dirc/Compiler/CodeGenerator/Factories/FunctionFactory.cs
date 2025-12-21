using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;
using Dirc.HAL;

namespace Dirc.Compiling.CodeGen;

class FunctionFactory
{
    private readonly ICodeGenBase _codeGenBase;
    private readonly Options _options;

    public FunctionFactory(ICodeGenBase codeGenBase, Options options)
    {
        _codeGenBase = codeGenBase;
        _options = options;
    }

    public void Generate(FunctionDeclarationNode node, CodeGenContext context)
    {
        CodeGenContext scopeSpecificContext = (CodeGenContext)context.GetSubcontext();

        _codeGenBase.EmitLabel(node.Name);
        _codeGenBase.EmitPush(new ReadonlyRegister(context.LR));
        _codeGenBase.EmitPush(new ReadonlyRegister(context.FP));
        _codeGenBase.EmitMov(new ReadonlyRegister(context.SP), context.Allocator.Use(context.FP));
        scopeSpecificContext.StackframeSize = CalculateStackframeSize(node.Body);
        AllocateStackframe(scopeSpecificContext);

        if (node.Parameters.Count > context.ArgumentRegisters.Count()) throw new Exception($"More than {context.ArgumentRegisters.Count()} function parameters given.");
        for (int i = 0; i < node.Parameters.Count; i++)
        {
            FunctionParameterNode parameter = node.Parameters[i];
            scopeSpecificContext.VariableTable[node.Parameters.Select(p => p.Name).ToList()[i]] = new RegisterStoredVariable(parameter.Name, scopeSpecificContext.Allocator.Use(context.ArgumentRegisters.ElementAt(i), false, true));
        }

        foreach (AstNode stmt in node.Body)
        {
            IReturnable? result = context.ExprFactory.Generate(stmt, scopeSpecificContext);
            result?.Free();
        }

        EmitFunctionEpilogue(context);
        _codeGenBase.EmitReturn();
    }

    public IReturnable? GenerateReturnStatement(ReturnStatementNode node, CodeGenContext context)
    {
        if (node.ReturnValue == null)
        {
            EmitFunctionEpilogue(context);
            _codeGenBase.EmitReturn(false);
            return null;
        }

        IReturnable returnValue = context.ExprFactory.Generate(node.ReturnValue, context) ?? throw new Exception("return value didn't return anything");
        Register returnReg = context.Allocator.Use(context.ReturnRegister, true);
        _codeGenBase.EmitMov(returnValue, returnReg);
        returnValue.Free();

        EmitFunctionEpilogue(context);
        _codeGenBase.EmitReturn(false);

        return new ReturnRegister(returnReg);
    }

    private void EmitFunctionEpilogue(CodeGenContext context)
    {
        // Free local variables
        _codeGenBase.EmitMov(new ReadonlyRegister(context.FP), context.Allocator.Use(context.SP));
        // Restore fp and lr
        _codeGenBase.EmitPop(context.Allocator.Use(context.FP));
        _codeGenBase.EmitPop(context.Allocator.Use(context.LR));
    }

    public void AllocateStackframe(CodeGenContext context)
    {
        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            new ReadonlyRegister(context.SP),
            new NumberLiteralNode(context.Options.TargetArchitecture.StackAlignment * context.StackframeSize),
            context.Allocator.Use(context.SP)
        );
    }

    public int CalculateStackframeSize(List<AstNode> body)
    {
        var allNodes = Helpers.GetAllDescendantNodes(body);
        int variableDeclarationCount = 0;

        variableDeclarationCount += allNodes.Count(n => n is VariableDeclarationNode);

        foreach (var arrNode in allNodes.OfType<ArrayDeclarationNode>())
        {
            variableDeclarationCount += arrNode.TotalSize();
        }

        foreach (var arrLiteralNode in allNodes.OfType<ArrayLiteralNode>())
        {
            variableDeclarationCount += arrLiteralNode.Elements.Count;
        }

        foreach (var strLiteralNode in allNodes.OfType<StringLiteralNode>())
        {
            variableDeclarationCount += strLiteralNode.Str.Literal!.ToString()!.Length + 1;
        }

        return variableDeclarationCount;
    }
}
