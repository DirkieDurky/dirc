using DircCompiler.Parsing;

namespace DircCompiler.CodeGen;

class ExpressionFactory
{
    private readonly CompilerOptions _compilerOptions;
    private readonly LabelGenerator _labelGenerator;

    public ExpressionFactory(CompilerOptions compilerOptions, LabelGenerator labelGenerator)
    {
        _compilerOptions = compilerOptions;
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
            case IfStatementNode ifStmt:
                return context.ControlFlowFactory.GenerateIfStatement(ifStmt, context, _labelGenerator);
            case WhileStatementNode whileStmt:
                return context.ControlFlowFactory.GenerateWhileStatement(whileStmt, context, _labelGenerator);
            case ReturnStatementNode returnStmt:
                return context.FunctionFactory.GenerateReturnStatement(returnStmt, context);
            case ArrayDeclarationNode arrayDecl:
                return context.ArrayFactory.GenerateArrayDeclaration(arrayDecl, context);
            case ArrayLiteralNode:
                // Array literals should be handled by the receiver of the array
                return null;
            case ArrayAccessNode arrayAccess:
                return context.ArrayFactory.GenerateArrayAccess(arrayAccess, context);
            case ArrayAssignmentNode arrayAssign:
                return context.ArrayFactory.GenerateArrayAssignment(arrayAssign, context);
            case AddressOfNode addressOf:
                return context.PointerFactory.GenerateAddressOf(addressOf, context);
            case PointerDereferenceNode deref:
                return context.PointerFactory.GeneratePointerDereference(deref, context);
            default:
                throw new Exception($"Unhandled node {node}");
        }
    }
}
