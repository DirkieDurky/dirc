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

            if (node.Value is ArrayLiteralNode arrayLiteral)
            {
                context.ArrayFactory.GenerateArrayLiteralAtPtr(arrayLiteral, address, context);
                address.Free();
                return null;
            }
            else
            {
                IReturnable value = context.ExprFactory.Generate(node.Value, context) ?? throw new Exception("Initializer expression failed to generate");
                _codeGenBase.EmitStore(value, address);
                address.Free();
                value.Free();
                return value;
            }

        }
        else
        {
            throw new CodeGenException("Invalid node type of left side of variable assignment", node.TargetName, context.Options, context.BuildContext);
        }
    }

    public IReturnable? AssignVariable(string name, AstNode assignment, CodeGenContext context)
    {
        Variable var = context.VariableTable[name];

        if (var is StackStoredVariable stackVar)
        {
            int offset = stackVar.FramePointerOffset;

            Register basePtr = context.Allocator.Allocate(Allocator.RegisterType.CallerSaved);

            _codeGenBase.EmitBinaryOperation(
                Operation.Sub,
                ReadonlyRegister.FP,
                new NumberLiteralNode(NumberLiteralType.Decimal, offset.ToString()),
                basePtr
            );

            if (assignment is ArrayLiteralNode arrayLiteral)
            {
                context.ArrayFactory.GenerateArrayLiteralAtPtr(arrayLiteral, new ReadonlyRegister(basePtr), context);
                return new ReturnRegister(basePtr);
            }
            else if (assignment is ArrayAccessNode arrayAccess && stackVar.IsArray)
            {
                int newOffset = 0;

                ArrayAccessNode currentArrayAccess = arrayAccess;
                while (currentArrayAccess.Array is ArrayAccessNode)
                {
                    newOffset -= int.Parse(((NumberLiteralNode)currentArrayAccess.Index).Value);
                    currentArrayAccess = (ArrayAccessNode)currentArrayAccess.Array;
                }
                var originalArray = context.VariableTable[((IdentifierNode)currentArrayAccess.Array).Name];
                if (originalArray is not StackStoredVariable stackArray)
                {
                    // TODO: Implement a way to do this
                    throw new NotImplementedException("Can't assign an value from an array to another array when array is stored in Register");
                }
                newOffset += stackArray.FramePointerOffset;
                newOffset -= int.Parse(((NumberLiteralNode)currentArrayAccess.Index).Value);
                context.VariableTable[name] = new StackStoredVariable(name, newOffset);
                return null;
            }
            else
            {
                IReturnable resultValue = context.ExprFactory.Generate(assignment, context) ?? throw new Exception("Initializer expression failed to generate");
                _codeGenBase.EmitStore(resultValue, new ReadonlyRegister(basePtr));
                basePtr.Free();

                return resultValue;
            }
        }
        else if (var is RegisterStoredVariable regVar)
        {
            IReturnable resultValue = context.ExprFactory.Generate(assignment, context) ?? throw new Exception("Initializer expression failed to generate");
            _codeGenBase.EmitMov(resultValue, regVar.Register);
            resultValue.Free();

            return resultValue;
        }
        else
        {
            throw new CodeGenException("Variable was of unsupported type", null, context.Options, context.BuildContext);
        }
    }
}
