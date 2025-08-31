using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of control flow statements (if, while, for) and conditions
/// </summary>
internal class ControlFlowParser
{
    private readonly ParserBase _parser;
    private readonly ExpressionParser _expressionParser;
    private VariableParser _variableParser;
    private StatementParser _statementParser;

    public ControlFlowParser(ParserBase parser, StatementParser statementParser, VariableParser variableParser)
    {
        _parser = parser;
        _expressionParser = new ExpressionParser(parser);
        _statementParser = statementParser;
        _variableParser = variableParser;
    }

    public List<AstNode> ParseIfStatement()
    {
        _parser.Consume(TokenType.LeftParen, "Expected '(' after if keyword");
        AstNode condition = ParseCondition();
        _parser.Consume(TokenType.RightParen, "Expected ')' after if condition");

        List<AstNode> body = ParseBody("if statement");
        List<AstNode>? elseBody = null;

        if (_parser.Match(TokenType.Else))
            elseBody = ParseBody("else statement");

        return [new IfStatementNode(condition, body, elseBody)];
    }

    public List<AstNode> ParseWhileStatement()
    {
        _parser.Consume(TokenType.LeftParen, "Expected '(' after while keyword");
        AstNode condition = ParseCondition();
        _parser.Consume(TokenType.RightParen, "Expected ')' after while condition");

        List<AstNode> body = ParseBody("while statement");
        return [new WhileStatementNode(condition, body)];
    }

    public List<AstNode> ParseForStatement()
    {
        _parser.Consume(TokenType.LeftParen, "Expected '(' after for keyword");

        // Parse initialization
        AstNode? initial = null;
        if (!_parser.Match(TokenType.Semicolon))
        {
            initial = _parser.Check(TokenType.Identifier) && _parser.CheckNext(TokenType.Identifier)
                ? _variableParser.ParseVariableDeclaration()
                : _expressionParser.ParseExpression();
            _parser.Consume(TokenType.Semicolon, "Expected ';' after for initialization");
        }

        // Parse condition
        AstNode? condition = null;
        if (!_parser.Match(TokenType.Semicolon))
        {
            condition = ParseCondition();
            _parser.Consume(TokenType.Semicolon, "Expected ';' after for condition");
        }

        // Parse increment
        AstNode? increment = null;
        if (!_parser.Match(TokenType.RightParen))
        {
            increment = _expressionParser.ParseExpression();
            _parser.Consume(TokenType.RightParen, "Expected ')' after for increment");
        }

        List<AstNode> body = ParseBody("for statement");
        if (increment != null)
            body.Add(increment);

        List<AstNode> result = [];
        if (initial != null)
            result.Add(initial);
        result.Add(new WhileStatementNode(condition ?? new BooleanLiteralNode(true), body));

        return result;
    }

    private AstNode ParseCondition()
    {
        AstNode expr = _expressionParser.ParseExpression();
        while (_parser.Check(TokenType.EqualEqual) || _parser.Check(TokenType.NotEqual) ||
               _parser.Check(TokenType.Less) || _parser.Check(TokenType.LessEqual) ||
               _parser.Check(TokenType.Greater) || _parser.Check(TokenType.GreaterEqual))
        {
            Token opToken = _parser.Advance();
            Comparer comparer = GetComparer(opToken);
            AstNode right = _expressionParser.ParseExpression();
            expr = new BinaryExpressionNode(comparer.ToOperation(), expr, right);
        }
        return expr;
    }

    private List<AstNode> ParseBody(string kind)
    {
        _parser.Consume(TokenType.LeftBrace, $"Expected '{{' after {kind}");
        List<AstNode> body = new();
        while (!_parser.Check(TokenType.RightBrace) && !_parser.IsAtEnd())
        {
            body.AddRange(_statementParser.ParseStatement());
        }
        _parser.Consume(TokenType.RightBrace, $"Expected '}}' after {kind} body");
        return body;
    }

    private Comparer GetComparer(Token token) => token.Type switch
    {
        TokenType.EqualEqual => Comparer.IfEq,
        TokenType.NotEqual => Comparer.IfNotEq,
        TokenType.Less => Comparer.IfLess,
        TokenType.LessEqual => Comparer.IfLessOrEq,
        TokenType.Greater => Comparer.IfMore,
        TokenType.GreaterEqual => Comparer.IfMoreOrEq,
        _ => throw new SyntaxException("Invalid comparer specified", token, _parser.Options, _parser.Context)
    };
}
