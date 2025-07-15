using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class FunctionCodeFactory
{
    private readonly CompilerOptions _compilerOptions;

    public FunctionCodeFactory(CompilerOptions compilerOptions)
    {
        _compilerOptions = compilerOptions;
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
            scopeSpecificContext.SymbolTable[node.Parameters[i]] = context.Allocator.Use(Allocator.ArgumentRegisters.ElementAt(i));
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

    public void CompileStandardFunction(CodeGenContext context, StandardFunction function)
    {
        context.CodeGen.EmitLabel(function.Name);
        foreach (string line in function.Code)
        {
            context.CodeGen.Emit(line);
        }
        context.CodeGen.EmitReturn();

        context.FunctionTable.Declare(new Function(function.Name, function.Parameters, false), null);
    }
}
