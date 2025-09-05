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

        CodeGenContext scopeSpecificContext = (CodeGenContext)context.CloneAndResetAllocator();
        if (node.Parameters.Count > Allocator.ArgumentRegisters.Count) throw new Exception($"More than {Allocator.ArgumentRegisters.Count} function parameters given.");
        for (int i = 0; i < node.Parameters.Count; i++)
        {
            scopeSpecificContext.RegisterTable[node.Parameters.Select(p => p.Name).ToList()[i]] = scopeSpecificContext.Allocator.Use(Allocator.ArgumentRegisters.ElementAt(i));
        }

        foreach (AstNode stmt in node.Body)
        {
            IReturnable? result = context.ExprFactory.Generate(stmt, scopeSpecificContext);
            result?.Free();
        }

        // Reset stack pointer to free any local variables
        _codeGenBase.EmitMov(ReadonlyRegister.FP, context.Allocator.Use(RegisterEnum.sp));
        _codeGenBase.EmitPop(context.Allocator.Use(RegisterEnum.fp));
        _codeGenBase.EmitPop(context.Allocator.Use(RegisterEnum.lr));
        _codeGenBase.EmitReturn();
    }

    public IReturnable? GenerateReturnStatement(ReturnStatementNode node, CodeGenContext context)
    {
        IReturnable returnValue = context.ExprFactory.Generate(node.ReturnValue, context) ?? throw new Exception("return value didn't return anything");
        Register r0 = context.Allocator.Use(RegisterEnum.r0, true);
        _codeGenBase.EmitMov(returnValue, r0);
        returnValue.Free();
        _codeGenBase.EmitReturn(false);

        return new ReturnRegister(r0);
    }
}
