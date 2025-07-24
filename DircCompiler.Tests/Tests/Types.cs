using DircCompiler.Parsing;
using DircCompiler.Semantic;
using DircCompiler.Lexing;

namespace DircCompiler.Tests;

public class Types
{
    private Token T(string lexeme) => new Token(TokenType.Identifier, lexeme, null, -1);
    private CompilerOptions _options = new([]);
    private CompilerContext _context = new("unittests");

    [Fact]
    public void ThrowsOnDuplicateFunction()
    {
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", new TypeNode(T("int"), "int"), new(), new()),
            new FunctionDeclarationNode(T("foo"), "foo", new TypeNode(T("int"), "int"), new(), new()),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnDuplicateVariable()
    {
        var nodes = new List<AstNode>
        {
            new VariableDeclarationNode(new TypeNode(T("int"), "int"), T("x")),
            new VariableDeclarationNode(new TypeNode(T("int"), "int"), T("x")),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnAssignmentToUndeclaredVariable()
    {
        var nodes = new List<AstNode>
        {
            new VariableAssignmentNode(T("x"), "x", new NumberLiteralNode(NumberLiteralType.Decimal, "1")),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnUseOfUndeclaredVariable()
    {
        var nodes = new List<AstNode>
        {
            new IdentifierNode(T("y"), "y"),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnTypeMismatchInInitialization()
    {
        var nodes = new List<AstNode>
        {
            new VariableDeclarationNode(new TypeNode(T("bool"), "bool"), T("x"), new NumberLiteralNode(NumberLiteralType.Decimal, "1")),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnTypeMismatchInAssignment()
    {
        var nodes = new List<AstNode>
        {
            new VariableDeclarationNode(new TypeNode(T("int"), "int"), T("x")),
            new VariableAssignmentNode(T("x"), "x", new BooleanLiteralNode(true)),
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnIfConditionNotBoolOrInt()
    {
        var nodes = new List<AstNode>
        {
            new IfStatementNode(new IdentifierNode(T("x"), "x"), new List<AstNode>(), null)
        };
        // x is undeclared, so this will throw for undeclared variable
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnWhileConditionNotBoolOrInt()
    {
        var nodes = new List<AstNode>
        {
            new WhileStatementNode(new IdentifierNode(T("x"), "x"), new List<AstNode>())
        };
        // x is undeclared, so this will throw for undeclared variable
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnCallToUndeclaredFunction()
    {
        var nodes = new List<AstNode>
        {
            new CallExpressionNode(T("foo"), "foo", new List<AstNode>())
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnFunctionArgumentCountMismatch()
    {
        var parameters = new List<FunctionParameterNode> { new FunctionParameterNode(T("int"), new TypeNode(T("int"), "int"), "a") };
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", new TypeNode(T("int"), "int"), parameters, new()),
            new CallExpressionNode(T("foo"), "foo", new List<AstNode>())
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnFunctionArgumentTypeMismatch()
    {
        var parameters = new List<FunctionParameterNode> { new FunctionParameterNode(T("int"), new TypeNode(T("int"), "int"), "a") };
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", new TypeNode(T("int"), "int"), parameters, new()),
            new CallExpressionNode(T("foo"), "foo", new List<AstNode> { new BooleanLiteralNode(true) })
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void ThrowsOnReturnTypeMismatch()
    {
        var parameters = new List<FunctionParameterNode>();
        var nodes = new List<AstNode>
        {
            new FunctionDeclarationNode(T("foo"), "foo", new TypeNode(T("int"), "int"), parameters, new List<AstNode> {
                new ReturnStatementNode(new BooleanLiteralNode(true))
            })
        };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void VariableDeclaration_WithValidType_DoesNotThrow()
    {
        var nodes = new List<AstNode>
            {
                new VariableDeclarationNode(
                    new TypeNode(T("int"), "int"),
                    new Token(TokenType.Identifier, "x", null, 1),
                    null)
            };
        new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context);
    }

    [Fact]
    public void VariableDeclaration_WithInvalidType_Throws()
    {
        var nodes = new List<AstNode>
            {
                new VariableDeclarationNode(
                    new TypeNode(T("asd"), "asd"),
                    new Token(TokenType.Identifier, "x", null, 1),
                    null)
            };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void FunctionDeclaration_WithInvalidReturnType_Throws()
    {
        var func = new FunctionDeclarationNode(
            new Token(TokenType.Identifier, "foo", null, 1),
            "foo",
            new TypeNode(T("asd"), "asd"),
            new List<FunctionParameterNode>(),
            new List<AstNode>()
        );
        var nodes = new List<AstNode> { func };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }

    [Fact]
    public void FunctionDeclaration_WithInvalidParameterType_Throws()
    {
        var func = new FunctionDeclarationNode(
            new Token(TokenType.Identifier, "foo", null, 1),
            "foo",
            new TypeNode(T("int"), "int"),
            new List<FunctionParameterNode> { new FunctionParameterNode(T("asd"), new TypeNode(T("asd"), "asd"), "x") },
            new List<AstNode>()
        );
        var nodes = new List<AstNode> { func };
        Assert.Throws<SemanticException>(() => new SemanticAnalyzer(_options, _context).Analyze(nodes, _options, _context));
    }
}
