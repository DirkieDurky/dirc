class FunctionCodeFactory
{
    public void Generate(FunctionDeclarationNode node, CodeGenContext context)
    {
        context.CodeGen.EmitLabel(node.Name);
        context.CodeGen.EmitPush(Register.LR);

        Register framePointer = context.Allocator.Allocate(Allocator.RegisterType.CalleeSaved);
        context.CodeGen.EmitMov(Register.SP, framePointer);

        CodeGenContext scopeSpecificContext = (CodeGenContext)context.Clone();
        scopeSpecificContext.FramePointer = framePointer;
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
        context.CodeGen.EmitMov(framePointer, Register.SP);
        context.Allocator.Free(framePointer);
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
