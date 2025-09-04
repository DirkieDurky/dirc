using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class VariableFactory
{
    private readonly CodeGenBase _codeGenBase;

    public VariableFactory(CodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

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

            _codeGenBase.EmitStore(value, address);
            address.Free();
            value.Free();

            return value;
        }
        else
        {
            throw new CodeGenException("Invalid node type of left side of variable assignment", node.TargetName, context.Options, context.BuildContext);
        }
    }

    public IReturnable? AssignVariable(int offset, AstNode assignment, CodeGenContext context)
    {
        IReturnable value = context.ExprFactory.Generate(assignment, context) ?? throw new Exception("Initializer expression failed to generate");
        Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

        _codeGenBase.EmitBinaryOperation(
            Operation.Sub,
            ReadonlyRegister.FP,
            new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()),
            tmp
        );
        _codeGenBase.EmitStore(value, new ReadonlyRegister(tmp));

        tmp.Free();
        value.Free();

        return value;
    }
}
