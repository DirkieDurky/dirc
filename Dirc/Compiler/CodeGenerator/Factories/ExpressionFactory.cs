using System.Runtime.Serialization;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class ExpressionFactory
{
    private readonly CodeGenBase _codeGenBase;
    private readonly LabelGenerator _labelGenerator;

    public ExpressionFactory(CodeGenBase codeGenBase, LabelGenerator labelGenerator)
    {
        _codeGenBase = codeGenBase;
        _labelGenerator = labelGenerator;
    }

    public IReturnable? Generate(AstNode node, CodeGenContext context)
    {
        switch (node)
        {
            case ExpressionStatementNode exprStmt:
                return Generate(exprStmt.Expression, context);
            case VariableDeclarationNode varDeclarationNode:
                return context.VarFactory.GenerateVariableDeclaration(varDeclarationNode, context);
            case VariableAssignmentNode varAssignmentNode:
                return context.VarFactory.GenerateVariableAssignment(varAssignmentNode, context);
            case CallExpressionNode call:
                return context.CallFactory.Generate(call, context);
            case BinaryExpressionNode bin:
                return context.BinaryFactory.Generate(bin, context, _labelGenerator);
            case IdentifierNode id:
                return context.IdentifierFactory.Generate(id, context);
            case BooleanLiteralNode boolean:
                return boolean;
            case NumberLiteralNode number:
                return number;
            case CharNode charNode:
                return new NumberLiteralNode(charNode.Value);
            case IfStatementNode ifStmt:
                return context.ControlFlowFactory.GenerateIfStatement(ifStmt, context, _labelGenerator);
            case WhileStatementNode whileStmt:
                return context.ControlFlowFactory.GenerateWhileStatement(whileStmt, context, _labelGenerator);
            case ForStatementNode forStmt:
                return context.ControlFlowFactory.GenerateForStatement(forStmt, context, _labelGenerator);
            case ReturnStatementNode returnStmt:
                return context.FunctionFactory.GenerateReturnStatement(returnStmt, context);
            case ArrayDeclarationNode arrayDecl:
                return context.ArrayFactory.GenerateArrayDeclaration(arrayDecl, context);
            case ArrayLiteralNode arrayLiteral:
                return context.ArrayFactory.GenerateArrayLiteralReturnBasePtr(arrayLiteral, context);
            case ArrayAccessNode arrayAccess:
                return context.ArrayFactory.GenerateArrayAccess(arrayAccess, context);
            case ArrayAssignmentNode arrayAssign:
                return context.ArrayFactory.GenerateArrayAssignment(arrayAssign, context);
            case AddressOfNode addressOf:
                return context.PointerFactory.GenerateAddressOf(addressOf, context);
            case PointerDereferenceNode deref:
                return context.PointerFactory.GeneratePointerDereference(deref, context);
            case StringLiteralNode stringLiteral:
                return context.StringFactory.GenerateStringLiteralReturnBasePtr(stringLiteral, context);
            case ArrLenNode arrLenNode:
                return new NumberLiteralNode(arrLenNode.ComputedLength);
            case AsmNode asmNode:
                context.CodeGenBase.Emit(asmNode.Code);
                return null;
            case UnaryOperationNode unaryOperationNode:
                return context.UnaryFactory.Generate(unaryOperationNode, context);
            default:
                throw new Exception($"Unhandled node {node}");
        }
    }
}
