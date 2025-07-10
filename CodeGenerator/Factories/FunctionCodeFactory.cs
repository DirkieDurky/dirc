class FunctionCodeFactory
{
    public void Generate(FunctionDeclarationNode node, CodeGenContext context)
    {
        context.CodeGen.EmitLabel(node.Name);
        context.CodeGen.EmitPush(new Register(RegisterEnum.lr));

        RegisterEnum framePointer = Allocator.Allocate(Allocator.RegisterType.CalleeSaved);
        context.CodeGen.EmitMov(new Register(RegisterEnum.sp), framePointer);

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
        context.CodeGen.EmitPop(RegisterEnum.lr);
        context.CodeGen.EmitReturn();
        context.CodeGen.Emit("");

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
