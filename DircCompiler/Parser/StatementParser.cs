using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Handles parsing of statements including declarations, control flow, and variable assignments
/// </summary>
internal class StatementParser
{
    private readonly ParserBase _parser;
    private readonly ExpressionParser _expressionParser;
    private readonly TypeParser _typeParser;

    public StatementParser(ParserBase parser)
    {
        _parser = parser;
        _expressionParser = new ExpressionParser(_parser);
        _typeParser = new TypeParser(_parser);
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
            return ParseIfStatement();

        if (_parser.Match(TokenType.While))
            return ParseWhileStatement();

        if (_parser.Match(TokenType.For))
            return ParseForStatement();

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
        _parser.Advance(); // Move past first identifier
        _parser.Advance(); // Move past second identifier or asterisk

        if (_parser.Check(TokenType.LeftParen))
        {
            _parser.Rewind(); // Move back to start of declaration
            _parser.Rewind();
            return [ParseFunctionDeclaration()];
        }
        else if (_parser.Check(TokenType.LeftBracket))
        {
            _parser.Rewind();
            _parser.Rewind();
            AstNode node = ParseArrayDeclaration();
            _parser.Consume(TokenType.Semicolon, "Expected ';' after array declaration");
            return [node];
        }
        else
        {
            _parser.Rewind();
            _parser.Rewind();
            VariableDeclarationNode node = ParseVariableDeclaration();
            _parser.Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
            return [node];
        }
    }

    private List<AstNode> ParseIfStatement()
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

    private List<AstNode> ParseWhileStatement()
    {
        _parser.Consume(TokenType.LeftParen, "Expected '(' after while keyword");
        AstNode condition = ParseCondition();
        _parser.Consume(TokenType.RightParen, "Expected ')' after while condition");

        List<AstNode> body = ParseBody("while statement");
        return [new WhileStatementNode(condition, body)];
    }

    private List<AstNode> ParseForStatement()
    {
        _parser.Consume(TokenType.LeftParen, "Expected '(' after for keyword");

        // Parse initialization
        AstNode? initial = null;
        if (!_parser.Match(TokenType.Semicolon))
        {
            initial = _parser.Check(TokenType.Identifier) && _parser.CheckNext(TokenType.Identifier)
                ? ParseVariableDeclaration()
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

    private List<AstNode> ParsePointerAssignment()
    {
        VariableAssignmentNode node = ParseVariableAssignment();
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
        VariableAssignmentNode node = ParseVariableAssignment();
        _parser.Consume(TokenType.Semicolon, "Expected ';' after variable assignment");
        return [node];
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

    private List<AstNode> ParseBody(string kind)
    {
        _parser.Consume(TokenType.LeftBrace, $"Expected '{{' after {kind}");
        List<AstNode> body = new();
        while (!_parser.Check(TokenType.RightBrace) && !_parser.IsAtEnd())
        {
            body.AddRange(ParseStatement());
        }
        _parser.Consume(TokenType.RightBrace, $"Expected '}}' after {kind} body");
        return body;
    }

    private FunctionDeclarationNode ParseFunctionDeclaration()
    {
        TypeNode returnType = _typeParser.ParseType();
        Token name = _parser.Advance();
        _parser.Consume(TokenType.LeftParen, "Expected '(' after function name");

        List<FunctionParameterNode> parameters = ParseFunctionParameters();
        _parser.Consume(TokenType.RightParen, "Expected ')' after parameters");

        return new FunctionDeclarationNode(name, name.Lexeme, returnType, parameters, ParseBody("function"));
    }

    private List<FunctionParameterNode> ParseFunctionParameters()
    {
        List<FunctionParameterNode> parameters = new();
        if (!_parser.Check(TokenType.RightParen))
        {
            do
            {
                TypeNode paramType = _typeParser.ParseType();
                Token paramName = _parser.Consume(TokenType.Identifier, "No parameter name provided");
                parameters.Add(new FunctionParameterNode(paramName, paramType, paramName.Lexeme));
            } while (_parser.Match(TokenType.Comma));
        }
        return parameters;
    }

    private VariableDeclarationNode ParseVariableDeclaration()
    {
        TypeNode type = _typeParser.ParseType();
        Token name = _parser.Advance();

        AstNode? initializer = null;
        if (_parser.Match(TokenType.Equals))
            initializer = _expressionParser.ParseExpression();

        return new VariableDeclarationNode(type, name, initializer);
    }

    private ArrayDeclarationNode ParseArrayDeclaration()
    {
        TypeNode type = _typeParser.ParseType();
        Token name = _parser.Advance();

        _parser.Consume(TokenType.LeftBracket, "Expected '[' in array declaration");
        AstNode size = _expressionParser.ParseExpression();
        _parser.Consume(TokenType.RightBracket, "Expected ']' after array size");

        AstNode? initializer = null;
        if (_parser.Match(TokenType.Equals))
            initializer = ParseArrayLiteral();

        return new ArrayDeclarationNode(type, name, size, initializer);
    }

    private ArrayLiteralNode ParseArrayLiteral()
    {
        _parser.Consume(TokenType.LeftBrace, "Expected '{' at start of array literal");
        List<AstNode> elements = new();

        if (!_parser.Check(TokenType.RightBrace))
        {
            do
            {
                elements.Add(_expressionParser.ParseExpression());
            } while (_parser.Match(TokenType.Comma));
        }

        _parser.Consume(TokenType.RightBrace, "Expected '}' at end of array literal");
        return new ArrayLiteralNode(elements);
    }

    internal VariableAssignmentNode ParseVariableAssignment()
    {
        AstNode target;
        Token? targetName = null;

        var pointerDereference = ParsePointerDereference();
        if (pointerDereference != null)
        {
            target = pointerDereference;
        }
        else
        {
            Token identifierToken = _parser.Consume(TokenType.Identifier, "Expected identifier in variable assignment");
            target = new IdentifierNode(identifierToken, identifierToken.Lexeme);
            targetName = identifierToken;
        }

        if (_parser.Match(TokenType.Equals))
        {
            AstNode assignmentValue = _expressionParser.ParseExpression();
            return new VariableAssignmentNode(target, targetName, assignmentValue);
        }

        if (_parser.Check(TokenType.Plus) && _parser.CheckNext(TokenType.Plus))
        {
            _parser.Advance(); // Skip Plus
            _parser.Advance(); // Skip Plus
            return new VariableAssignmentNode(target, targetName,
                new BinaryExpressionNode(Operation.Add, target, new NumberLiteralNode(1)));
        }

        if (_parser.Check(TokenType.Minus) && _parser.CheckNext(TokenType.Minus))
        {
            _parser.Advance(); // Skip Minus
            _parser.Advance(); // Skip Minus
            return new VariableAssignmentNode(target, targetName,
                new BinaryExpressionNode(Operation.Sub, target, new NumberLiteralNode(1)));
        }

        Operation op = GetOperation(_parser.Advance());
        _parser.Consume(TokenType.Equals, "Expected '=' after operation in variable assignment");

        AstNode value = _expressionParser.ParseExpression();
        return new VariableAssignmentNode(target, targetName,
            new BinaryExpressionNode(op, target, value));
    }

    private Operation GetOperation(Token token)
    {
        var operations = new Dictionary<TokenType, Operation>
        {
            { TokenType.Plus, Operation.Add },
            { TokenType.Minus, Operation.Sub },
            { TokenType.Asterisk, Operation.Mul },
            { TokenType.Slash, Operation.Div },
            { TokenType.Pipe, Operation.Or },
            { TokenType.Ampersand, Operation.And },
            { TokenType.Caret, Operation.Xor },
        };

        if (!operations.ContainsKey(token.Type))
            throw new SyntaxException("Invalid operation specified", token, _parser.Options, _parser.Context);

        return operations[token.Type];
    }

    private AstNode? ParsePointerDereference()
    {
        if (!_parser.Match(TokenType.Asterisk))
            return null;

        AstNode expr;
        if (_parser.Match(TokenType.LeftParen))
        {
            expr = _expressionParser.ParseExpression();
            _parser.Consume(TokenType.RightParen, "Expected ')' after expression");
        }
        else
        {
            Token name = _parser.Consume(TokenType.Identifier, "Expected identifier after '*'");
            expr = new IdentifierNode(name, name.Lexeme);
        }
        return new PointerDereferenceNode(expr);
    }
}
