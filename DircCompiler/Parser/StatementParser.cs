using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Handles parsing of statements including declarations, control flow, and variable assignments
/// </summary>
class StatementParser
{
    private readonly ParserBase _parser;
    private readonly ExpressionParser _expressionParser;
    private readonly FunctionParser _functionParser;
    private readonly ControlFlowParser _controlFlowParser;
    private readonly VariableParser _variableParser;
    private readonly ArrayParser _arrayParser;
    private readonly PointerParser _pointerParser;

    public StatementParser(ParserBase parser)
    {
        _parser = parser;
        _expressionParser = new ExpressionParser(_parser);
        _variableParser = new VariableParser(_parser);
        _functionParser = new FunctionParser(_parser, this);
        _controlFlowParser = new ControlFlowParser(_parser, this, _variableParser);
        _arrayParser = new ArrayParser(_parser);
        _pointerParser = new PointerParser(_parser);
    }

    public List<AstNode> ParseStatement()
    {
        if (_parser.Match(TokenType.Return))
            return ParseReturnStatement();

        if (_parser.Match(TokenType.Import))
            return ParseImportStatement();

        // Function, array, or variable declarations
        if (_parser.Check(TokenType.Identifier) && (_parser.CheckNext(TokenType.Identifier) || _parser.CheckNext(TokenType.Asterisk)))
            return ParseDeclaration();

        if (_parser.Match(TokenType.If))
            return _controlFlowParser.ParseIfStatement();

        if (_parser.Match(TokenType.While))
            return _controlFlowParser.ParseWhileStatement();

        if (_parser.Match(TokenType.For))
            return _controlFlowParser.ParseForStatement();

        if (_parser.Check(TokenType.Asterisk))
            return ParsePointerAssignment();

        if (_parser.Check(TokenType.Identifier))
            return ParseIdentifierStatement();

        while (_parser.Match(TokenType.Semicolon)) { }

        if (_parser.IsAtEnd())
            return [];

        AstNode expr = _expressionParser.ParseExpression();
        _parser.Consume(TokenType.Semicolon, "Expected ';' after expression");
        return [new ExpressionStatementNode(expr)];
    }

    private List<AstNode> ParseReturnStatement()
    {
        ReturnStatementNode node = new(_expressionParser.ParseExpression());
        _parser.Consume(TokenType.Semicolon, "Expected ';' after expression");
        return [node];
    }

    private List<AstNode> ParseImportStatement()
    {
        Token name = _parser.Consume(TokenType.Identifier, "No import function name provided");
        _parser.Consume(TokenType.Semicolon, "Expected ';' after import");
        return [new ImportStatementNode(name, name.Lexeme)];
    }

    private List<AstNode> ParseDeclaration()
    {
        _parser.Advance();
        _parser.Advance();

        // Function declaration
        if (_parser.Check(TokenType.LeftParen))
        {
            _parser.Rewind();
            _parser.Rewind();
            return [_functionParser.ParseFunctionDeclaration()];
        }
        // Array declaration
        else if (_parser.Check(TokenType.LeftBracket))
        {
            _parser.Rewind();
            _parser.Rewind();
            AstNode node = _arrayParser.ParseArrayDeclaration();
            _parser.Consume(TokenType.Semicolon, "Expected ';' after array declaration");
            return [node];
        }
        // Variable declaration
        else
        {
            _parser.Rewind();
            _parser.Rewind();
            VariableDeclarationNode node = _variableParser.ParseVariableDeclaration();
            _parser.Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
            return [node];
        }
    }

    private List<AstNode> ParsePointerAssignment()
    {
        VariableAssignmentNode node = _variableParser.ParseVariableAssignment();
        _parser.Consume(TokenType.Semicolon, "Expected ';' after pointer assignment");
        return [node];
    }

    private List<AstNode> ParseIdentifierStatement()
    {
        if (_parser.CheckNext(TokenType.LeftBracket))
        {
            AstNode arrayNode = _expressionParser.ParseExpression();
            _parser.Consume(TokenType.Semicolon, "Expected ';' after array statement");
            return [arrayNode];
        }

        Token name = _parser.Advance();
        if (_parser.Match(TokenType.LeftParen))
        {
            AstNode functionCallNode = _expressionParser.ParseFunctionCall(name);
            _parser.Consume(TokenType.Semicolon, "Expected ';' after function call");
            return [functionCallNode];
        }

        _parser.Rewind();
        VariableAssignmentNode node = _variableParser.ParseVariableAssignment();
        _parser.Consume(TokenType.Semicolon, "Expected ';' after variable assignment");
        return [node];
    }
}
