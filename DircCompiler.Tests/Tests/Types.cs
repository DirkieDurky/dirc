using DircCompiler.Parsing;
using DircCompiler.Semantic;
using DircCompiler.Lexing;

namespace DircCompiler.Tests;

public class Types
{
    private Token T(string lexeme) => new Token(TokenType.Identifier, lexeme, null, 1);

    [Fact]
    public void ThrowsOnDuplicateFunction()
    {
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", T("int"), new(), new()),
            new FunctionDeclarationNode(T("foo"), "foo", T("int"), new(), new()),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnDuplicateVariable()
    {
        var nodes = new List<AstNode>
        {
            new VariableDeclarationNode(T("int"), T("x")),
            new VariableDeclarationNode(T("int"), T("x")),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnAssignmentToUndeclaredVariable()
    {
        var nodes = new List<AstNode>
        {
            new VariableAssignmentNode(T("x"), "x", new NumberLiteralNode(NumberLiteralType.Decimal, "1")),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnUseOfUndeclaredVariable()
    {
        var nodes = new List<AstNode>
        {
            new IdentifierNode(T("y"), "y"),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnTypeMismatchInInitialization()
    {
        var nodes = new List<AstNode>
        {
            new VariableDeclarationNode(T("bool"), T("x"), new NumberLiteralNode(NumberLiteralType.Decimal, "1")),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnTypeMismatchInAssignment()
    {
        var nodes = new List<AstNode>
        {
            new VariableDeclarationNode(T("int"), T("x")),
            new VariableAssignmentNode(T("x"), "x", new BooleanLiteralNode(true)),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnConditionOperandsNotInt()
    {
        var nodes = new List<AstNode>
        {
            new IfStatementNode(
                new ConditionNode(Comparer.IfEq, new BooleanLiteralNode(true), new BooleanLiteralNode(false)),
                new List<AstNode>(), null)
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnIfConditionNotBoolOrInt()
    {
        var nodes = new List<AstNode>
        {
            new IfStatementNode(new IdentifierNode(T("x"), "x"), new List<AstNode>(), null)
        };
        // x is undeclared, so this will throw for undeclared variable
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnWhileConditionNotBoolOrInt()
    {
        var nodes = new List<AstNode>
        {
            new WhileStatementNode(new IdentifierNode(T("x"), "x"), new List<AstNode>())
        };
        // x is undeclared, so this will throw for undeclared variable
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnCallToUndeclaredFunction()
    {
        var nodes = new List<AstNode>
        {
            new CallExpressionNode(T("foo"), "foo", new List<AstNode>())
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnFunctionArgumentCountMismatch()
    {
        var parameters = new List<FunctionParameter> { new FunctionParameter("int", "a") };
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", T("int"), parameters, new()),
            new CallExpressionNode(T("foo"), "foo", new List<AstNode>())
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnFunctionArgumentTypeMismatch()
    {
        var parameters = new List<FunctionParameter> { new FunctionParameter("int", "a") };
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", T("int"), parameters, new()),
            new CallExpressionNode(T("foo"), "foo", new List<AstNode> { new BooleanLiteralNode(true) })
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void ThrowsOnReturnTypeMismatch()
    {
        var parameters = new List<FunctionParameter>();
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", T("int"), parameters, new List<AstNode> {
                new ReturnStatementNode(new BooleanLiteralNode(true))
            })
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer().Analyze(nodes, new([]), new("unittests")));
    }
}
