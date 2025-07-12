class FunctionCodeFactory
{
    public void Generate(FunctionDeclarationNode node, CodeGenContext context)
    {
        context.CodeGen.EmitLabel(node.Name);
        context.CodeGen.EmitPush(Register.LR);
        context.CodeGen.EmitPush(Register.FP);
        context.CodeGen.EmitMov(Register.SP, Register.FP);

        CodeGenContext scopeSpecificContext = (CodeGenContext)context.Clone();
        if (node.Parameters.Count > Allocator.ArgumentRegisters.Count) throw new Exception($"No more than {Allocator.ArgumentRegisters.Count} function parameters allowed.");
        for (int i = 0; i < node.Parameters.Count; i++)
        {
            scopeSpecificContext.SymbolTable[node.Parameters[i]] = Allocator.ArgumentRegisters.ElementAt(i);
        }

        foreach (AstNode stmt in node.Body)
        {
            context.ExprFactory.Generate(stmt, scopeSpecificContext);
        }

        // Reset stack pointer to free any local variables
        context.CodeGen.EmitMov(Register.FP, Register.SP);
        context.CodeGen.EmitPop(Register.FP);
        context.CodeGen.EmitPop(Register.LR);
        context.CodeGen.EmitReturn();

        context.FunctionTable.Declare(Function.FromFunctionDeclarationNode(node));
    }

    public void CompileStandardFunction(CodeGenContext context, string functionName, string[] parameters, string[] code)
    {
        context.CodeGen.EmitLabel(functionName);
        foreach (string line in code)
        {
            context.CodeGen.Emit(line);
        }
        context.CodeGen.EmitReturn();

        context.FunctionTable.Declare(new Function(functionName, parameters, false));
    }
}
