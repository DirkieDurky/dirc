using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of statements including declarations, control flow, and variable assignments
/// </summary>
class StatementParser
{
    private readonly ParserContext _context;

    public StatementParser(ParserContext context)
    {
        _context = context;
    }

    public List<AstNode> ParseStatement()
    {
        if (_context.ParserBase.Match(TokenType.Return))
            return ParseReturnStatement();

        if (_context.ParserBase.Match(TokenType.Import))
            return ParseImportStatement();

        // Function, array, or variable declarations
        if (_context.TypeParser.TryParseType(out TypeNode? type))
        {
            return ParseDeclaration(type!);
        }

        if (_context.ParserBase.Match(TokenType.If))
            return _context.ControlFlowParser.ParseIfStatement();

        if (_context.ParserBase.Match(TokenType.While))
            return _context.ControlFlowParser.ParseWhileStatement();

        if (_context.ParserBase.Match(TokenType.For))
            return _context.ControlFlowParser.ParseForStatement();

        if (_context.ParserBase.Check(TokenType.Asterisk))
            return ParsePointerAssignment();

        if (_context.ParserBase.Check(TokenType.Identifier))
            return ParseIdentifierStatement();

        if (_context.ParserBase.Match(TokenType.Asm))
        {
            _context.ParserBase.Consume(TokenType.LeftParen, "Expected opening parenthesis in asm call");
            Token code = _context.ParserBase.Consume(TokenType.String, "Expected string in asm call");
            _context.ParserBase.Consume(TokenType.RightParen, "Expected closing parenthesis in asm call");
            _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after asm statement");
            return [new AsmNode((string)code.Literal!)];
        }

        while (_context.ParserBase.Match(TokenType.Semicolon)) { }

        if (_context.ParserBase.IsAtEnd())
            return [];

        AstNode expr = _context.ExpressionParser.ParseExpression();
        _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after expression");
        return [new ExpressionStatementNode(expr)];
    }

    private List<AstNode> ParseReturnStatement()
    {
        if (_context.ParserBase.Match(TokenType.Semicolon)) return [];
        ReturnStatementNode node = new(_context.ExpressionParser.ParseExpression());
        _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after expression");
        return [node];
    }

    private List<AstNode> ParseImportStatement()
    {
        Token name = _context.ParserBase.Consume(TokenType.Identifier, "No import function name provided");
        _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after import");
        return [new ImportStatementNode(name, name.Lexeme)];
    }

    private List<AstNode> ParseDeclaration(TypeNode type)
    {
        // Function declaration
        if (_context.ParserBase.CheckNext(TokenType.LeftParen))
        {
            return [_context.FunctionParser.ParseFunctionDeclaration(type)];
        }
        // Array declaration
        else if (type.IsArray)
        {
            AstNode node = _context.ArrayParser.ParseArrayDeclaration(type);
            _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after array declaration");
            return [node];
        }
        // Variable declaration
        else
        {
            VariableDeclarationNode node = _context.VariableParser.ParseVariableDeclaration(type);
            _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
            return [node];
        }
    }

    private List<AstNode> ParsePointerAssignment()
    {
        VariableAssignmentNode node = _context.VariableParser.ParseVariableAssignment();
        _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after pointer assignment");
        return [node];
    }

    private List<AstNode> ParseIdentifierStatement()
    {
        if (_context.ParserBase.CheckNext(TokenType.LeftBracket))
        {
            AstNode arrayNode = _context.ExpressionParser.ParseExpression();
            _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after array statement");
            return [arrayNode];
        }

        Token name = _context.ParserBase.Advance();
        if (_context.ParserBase.Match(TokenType.LeftParen))
        {
            AstNode functionCallNode = _context.ExpressionParser.ParseFunctionCall(name);
            _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after function call");
            return [functionCallNode];
        }

        _context.ParserBase.Rewind();
        VariableAssignmentNode node = _context.VariableParser.ParseVariableAssignment();
        _context.ParserBase.Consume(TokenType.Semicolon, "Expected ';' after variable assignment");
        return [node];
    }
}
