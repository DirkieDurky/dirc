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

    [Fact]
    public void VariableDeclaration_WithValidType_DoesNotThrow()
    {
        var nodes = new List<AstNode>
            {
                new VariableDeclarationNode(
                    new Token(TokenType.Identifier, "int", null, 1),
                    new Token(TokenType.Identifier, "x", null, 1),
                    null)
            };
        var analyzer = new SemanticAnalyzer();
        analyzer.Analyze(nodes, new([]), new("unittests"));
    }

    [Fact]
    public void VariableDeclaration_WithInvalidType_Throws()
    {
        var nodes = new List<AstNode>
            {
                new VariableDeclarationNode(
                    new Token(TokenType.Identifier, "asd", null, 1),
                    new Token(TokenType.Identifier, "x", null, 1),
                    null)
            };
        var analyzer = new SemanticAnalyzer();
        Assert.Throws<SemanticException>(() => analyzer.Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void FunctionDeclaration_WithInvalidReturnType_Throws()
    {
        var func = new FunctionDeclarationNode(
            new Token(TokenType.Identifier, "foo", null, 1),
            "foo",
            new Token(TokenType.Identifier, "asd", null, 1),
            new List<FunctionParameter>(),
            new List<AstNode>()
        );
        var nodes = new List<AstNode> { func };
        var analyzer = new SemanticAnalyzer();
        Assert.Throws<SemanticException>(() => analyzer.Analyze(nodes, new([]), new("unittests")));
    }

    [Fact]
    public void FunctionDeclaration_WithInvalidParameterType_Throws()
    {
        var func = new FunctionDeclarationNode(
            new Token(TokenType.Identifier, "foo", null, 1),
            "foo",
            new Token(TokenType.Identifier, "int", null, 1),
            new List<FunctionParameter> { new FunctionParameter("asd", "x") },
            new List<AstNode>()
        );
        var nodes = new List<AstNode> { func };
        var analyzer = new SemanticAnalyzer();
        Assert.Throws<SemanticException>(() => analyzer.Analyze(nodes, new([]), new("unittests")));
    }
}
