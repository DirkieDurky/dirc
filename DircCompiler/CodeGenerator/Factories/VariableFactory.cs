using DircCompiler.CodeGen.Allocating;
using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class VariableFactory
{
    public IReturnable? GenerateVariableDeclaration(VariableDeclarationNode node, CodeGenContext context)
    {
        int offset = context.AllocateVariable(node.Name);

        if (node.Initializer == null) return null;

        return AssignVariable(offset, node.Initializer, context);
    }

    public IReturnable? GenerateVariableAssignment(VariableAssignmentNode node, CodeGenContext context)
    {
        if (node.Target is IdentifierNode)
        {
            int offset = context.VariableTable[node.Name!].FramePointerOffset;

            return AssignVariable(offset, node.Value, context);
        }
        else if (node.Target is PointerDereferenceNode target)
        {
            IReturnable address = context.ExprFactory.Generate(target.PointerExpression, context) ?? throw new Exception("Pointer dereference failed to generate");
            IReturnable value = context.ExprFactory.Generate(node.Value, context) ?? throw new Exception("Initializer expression failed to generate");

            context.CodeGen.EmitStore(value, address);
            address.Free();
            value.Free();

            return value;
        }
        else
        {
            throw new CodeGenException("Invalid node type of left side of variable assignment", node.TargetName, context.CompilerOptions, context.CompilerContext);
        }
    }

    public IReturnable? AssignVariable(int offset, AstNode assignment, CodeGenContext context)
    {
        IReturnable value = context.ExprFactory.Generate(assignment, context) ?? throw new Exception("Initializer expression failed to generate");
        Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

        context.CodeGen.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()),
            tmp
        );
        context.CodeGen.EmitStore(value, new ReadonlyRegister(tmp));

        tmp.Free();
        value.Free();

        return value;
    }
}
