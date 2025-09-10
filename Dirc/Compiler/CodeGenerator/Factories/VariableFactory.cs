using System.Reflection.Metadata.Ecma335;
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
        context.AllocateStackVariable(node.Name);

        if (node.Initializer == null) return null;

        return AssignVariable(node.Name, node.Initializer, context);
    }

    public IReturnable? GenerateVariableAssignment(VariableAssignmentNode node, CodeGenContext context)
    {
        if (node.Target is IdentifierNode)
        {
            return AssignVariable(node.Name!, node.Value, context);
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

    public IReturnable? AssignVariable(string name, AstNode assignment, CodeGenContext context)
    {
        Variable var = context.VariableTable[name];
        IReturnable resultValue = context.ExprFactory.Generate(assignment, context) ?? throw new Exception("Initializer expression failed to generate");

        if (var is StackStoredVariable stackVar)
        {
            int offset = stackVar.FramePointerOffset;

            Register tmp = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

            _codeGenBase.EmitBinaryOperation(
                Operation.Sub,
                ReadonlyRegister.FP,
                new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()),
                tmp
            );
            _codeGenBase.EmitStore(resultValue, new ReadonlyRegister(tmp));

            tmp.Free();
        }
        else if (var is RegisterStoredVariable regVar)
        {
            _codeGenBase.EmitMov(resultValue, regVar.Register);
        }
        else
        {
            throw new CodeGenException("Variable was of unsupported type", null, context.Options, context.BuildContext);
        }

        resultValue.Free();
        return resultValue;
    }
}
