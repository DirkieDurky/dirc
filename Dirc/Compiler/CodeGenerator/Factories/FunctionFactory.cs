using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

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
        _codeGenBase.EmitPush(ReadonlyRegister.LR);
        _codeGenBase.EmitPush(ReadonlyRegister.FP);
        _codeGenBase.EmitMov(ReadonlyRegister.SP, context.Allocator.Use(RegisterEnum.fp));
        scopeSpecificContext.StackframeSize = CalculateStackframeSize(node.Body);
        AllocateStackframe(scopeSpecificContext);

        if (node.Parameters.Count > Allocator.ArgumentRegisters.Count) throw new Exception($"More than {Allocator.ArgumentRegisters.Count} function parameters given.");
        for (int i = 0; i < node.Parameters.Count; i++)
        {
            FunctionParameterNode parameter = node.Parameters[i];
            scopeSpecificContext.VariableTable[node.Parameters.Select(p => p.Name).ToList()[i]] = new RegisterStoredVariable(parameter.Name, scopeSpecificContext.Allocator.Use(Allocator.ArgumentRegisters.ElementAt(i), false, true));
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
        Register r0 = context.Allocator.Use(RegisterEnum.r0, true);
        _codeGenBase.EmitMov(returnValue, r0);
        returnValue.Free();

        EmitFunctionEpilogue(context);
        _codeGenBase.EmitReturn(false);

        return new ReturnRegister(r0);
    }

    private void EmitFunctionEpilogue(CodeGenContext context)
    {
        // Free local variables
        _codeGenBase.EmitMov(ReadonlyRegister.FP, context.Allocator.Use(RegisterEnum.sp));
        // Restore fp and lr
        _codeGenBase.EmitPop(context.Allocator.Use(RegisterEnum.fp));
        _codeGenBase.EmitPop(context.Allocator.Use(RegisterEnum.lr));
    }

    public void AllocateStackframe(CodeGenContext context)
    {
        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.SP,
            new NumberLiteralNode(context.BuildEnvironment.StackAlignment * context.StackframeSize),
            context.Allocator.Use(RegisterEnum.sp)
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
