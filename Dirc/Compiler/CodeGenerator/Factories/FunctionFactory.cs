using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class FunctionFactory
{
    private readonly CodeGenBase _codeGenBase;
    private readonly Options _options;

    public FunctionFactory(CodeGenBase codeGenBase, Options options)
    {
        _codeGenBase = codeGenBase;
        _options = options;
    }

    public void Generate(FunctionDeclarationNode node, CodeGenContext context)
    {
        _codeGenBase.EmitLabel(node.Name);
        _codeGenBase.EmitPush(ReadonlyRegister.LR);
        _codeGenBase.EmitPush(ReadonlyRegister.FP);
        _codeGenBase.EmitMov(ReadonlyRegister.SP, context.Allocator.Use(RegisterEnum.fp));
        AllocateSpaceForLocalVariables(node.Body, context);

        CodeGenContext scopeSpecificContext = (CodeGenContext)context.CloneAndResetAllocator();
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

    public void AllocateSpaceForLocalVariables(List<AstNode> body, CodeGenContext context)
    {
        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.SP,
            new NumberLiteralNode(BuildEnvironment.StackAlignment * GetLocalVariablesCount(body)),
            context.Allocator.Use(RegisterEnum.sp)
        );
    }

    public int GetLocalVariablesCount(List<AstNode> body)
    {
        int variableDeclarationCount = body.Count(n => n is VariableDeclarationNode);
        foreach (IfStatementNode ifNode in body.Where(n => n is IfStatementNode))
        {
            variableDeclarationCount += GetLocalVariablesCount(ifNode.Body);
        }
        foreach (WhileStatementNode whileNode in body.Where(n => n is WhileStatementNode))
        {
            variableDeclarationCount += GetLocalVariablesCount(whileNode.Body);
        }
        return variableDeclarationCount;
    }

    public IReturnable? GenerateReturnStatement(ReturnStatementNode node, CodeGenContext context)
    {
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
}
