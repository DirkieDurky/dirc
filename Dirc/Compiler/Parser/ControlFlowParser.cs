using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of control flow statements (if, while, for) and conditions
/// </summary>
internal class ControlFlowParser
{
    private readonly ParserContext _context;

    public ControlFlowParser(ParserContext context)
    {
        _context = context;
    }

    public List<AstNode> ParseIfStatement(bool isElseIf = false)
    {
        _context.ParserBase.Consume(TokenType.LeftParen, "Expected '(' after if keyword");
        AstNode condition = ParseCondition();
        _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after if condition");

        string bodyKind = isElseIf ? "else if statement" : "else statement";
        List<AstNode> body = ParseBody(bodyKind);
        List<AstNode>? elseBody = null;

        if (_context.ParserBase.Match(TokenType.Else))
        {
            elseBody = [];

            if (_context.ParserBase.Match(TokenType.If))
            {
                elseBody = ParseIfStatement(true);
            }
            else
            {
                elseBody.AddRange(ParseBody("else statement"));
            }
        }

        return [new IfStatementNode(condition, body, elseBody)];
    }

    public List<AstNode> ParseWhileStatement()
    {
        _context.ParserBase.Consume(TokenType.LeftParen, "Expected '(' after while keyword");
        AstNode condition = ParseCondition();
        _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after while condition");

        List<AstNode> body = ParseBody("while statement");
        return [new WhileStatementNode(condition, body)];
    }

    public List<AstNode> ParseForStatement()
    {
        _context.ParserBase.Consume(TokenType.LeftParen, "Expected '(' after for keyword");

        // Parse initialization
        AstNode? initial = null;
        if (!_context.ParserBase.Match(TokenType.Semicolon))
        {
            initial = _context.ParserBase.Check(TokenType.Identifier) && _context.ParserBase.CheckNext(TokenType.Identifier)
                ? _context.VariableParser.ParseVariableDeclaration()
                : _context.ExpressionParser.ParseExpression();
            _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after for initialization");
        }

        // Parse condition
        AstNode? condition = null;
        if (!_context.ParserBase.Match(TokenType.Semicolon))
        {
            condition = ParseCondition();
            _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after for condition");
        }

        // Parse increment
        AstNode? increment = null;
        if (!_context.ParserBase.Match(TokenType.RightParen))
        {
            increment = _context.ExpressionParser.ParseExpression();
            _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after for increment");
        }

        List<AstNode> body = ParseBody("for statement");

        return [new ForStatementNode(initial, condition, increment, body)];
    }

    private AstNode ParseCondition()
    {
        AstNode expr = _context.ExpressionParser.ParseExpression();
        while (_context.ParserBase.Check(TokenType.EqualEqual) || _context.ParserBase.Check(TokenType.NotEqual) ||
               _context.ParserBase.Check(TokenType.Less) || _context.ParserBase.Check(TokenType.LessEqual) ||
               _context.ParserBase.Check(TokenType.Greater) || _context.ParserBase.Check(TokenType.GreaterEqual))
        {
            Token opToken = _context.ParserBase.Advance();
            Comparer comparer = GetComparer(opToken);
            AstNode right = _context.ExpressionParser.ParseExpression();
            expr = new BinaryExpressionNode(comparer.ToOperation(), expr, right);
        }
        return expr;
    }

    private List<AstNode> ParseBody(string kind)
    {
        _context.ParserBase.Consume(TokenType.LeftBrace, $"Expected '{{' after {kind}");
        List<AstNode> body = new();
        while (!_context.ParserBase.Check(TokenType.RightBrace) && !_context.ParserBase.IsAtEnd())
        {
            body.AddRange(_context.StatementParser.ParseStatement());
        }
        _context.ParserBase.Consume(TokenType.RightBrace, $"Expected '}}' after {kind} body");
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
        _ => throw new SyntaxException("Invalid comparer specified", token, _context.ParserBase.Options, _context.ParserBase.Context)
    };
}
