using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class FunctionFactory
{
    private readonly BuildOptions _buildOptions;

    public FunctionFactory(BuildOptions buildOptions)
    {
        _buildOptions = buildOptions;
    }

    public void Generate(FunctionDeclarationNode node, CodeGenContext context)
    {
        context.CodeGen.EmitLabel(node.Name);
        context.CodeGen.EmitPush(ReadonlyRegister.LR);
        context.CodeGen.EmitPush(ReadonlyRegister.FP);
        context.CodeGen.EmitMov(ReadonlyRegister.SP, context.Allocator.Use(RegisterEnum.fp));

        CodeGenContext scopeSpecificContext = (CodeGenContext)context.Clone();
        if (node.Parameters.Count > Allocator.ArgumentRegisters.Count) throw new Exception($"More than {Allocator.ArgumentRegisters.Count} function parameters given.");
        for (int i = 0; i < node.Parameters.Count; i++)
        {
            scopeSpecificContext.SymbolTable[node.Parameters.Select(p => p.Name).ToList()[i]] = scopeSpecificContext.Allocator.Use(Allocator.ArgumentRegisters.ElementAt(i));
        }

        foreach (AstNode stmt in node.Body)
        {
            context.ExprFactory.Generate(stmt, scopeSpecificContext);
        }

        // Reset stack pointer to free any local variables
        context.CodeGen.EmitMov(ReadonlyRegister.FP, context.Allocator.Use(RegisterEnum.sp));
        context.CodeGen.EmitPop(context.Allocator.Use(RegisterEnum.fp));
        context.CodeGen.EmitPop(context.Allocator.Use(RegisterEnum.lr));
        context.CodeGen.EmitReturn();
    }

    public IReturnable? GenerateReturnStatement(ReturnStatementNode node, CodeGenContext context)
    {
        IReturnable returnValue = context.ExprFactory.Generate(node.ReturnValue, context) ?? throw new Exception("return value didn't return anything");
        Register r0 = context.Allocator.Use(RegisterEnum.r0, true);
        context.CodeGen.EmitMov(returnValue, r0);
        returnValue.Free();
        r0.Free();
        context.CodeGen.EmitReturn(false);

        return returnValue;
    }
}
