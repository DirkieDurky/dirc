using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Handles parsing of variable declarations and assignments
/// </summary>
internal class VariableParser
{
    private readonly ParserBase _parser;
    private readonly TypeParser _typeParser;
    private readonly ExpressionParser _expressionParser;
    private readonly PointerParser _pointerParser;

    public VariableParser(ParserBase parser)
    {
        _parser = parser;
        _typeParser = new TypeParser(parser);
        _expressionParser = new ExpressionParser(parser);
        _pointerParser = new PointerParser(parser);
    }

    public VariableDeclarationNode ParseVariableDeclaration()
    {
        TypeNode type = _typeParser.ParseType();
        Token name = _parser.Advance();

        AstNode? initializer = null;
        if (_parser.Match(TokenType.Equals))
            initializer = _expressionParser.ParseExpression();

        return new VariableDeclarationNode(type, name, initializer);
    }

    internal VariableAssignmentNode ParseVariableAssignment()
    {
        AstNode target;
        Token? targetName = null;

        var pointerDereference = _pointerParser.ParsePointerDereference();
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
            _parser.Advance();
            _parser.Advance();
            return new VariableAssignmentNode(target, targetName,
                new BinaryExpressionNode(Operation.Add, target, new NumberLiteralNode(1)));
        }

        if (_parser.Check(TokenType.Minus) && _parser.CheckNext(TokenType.Minus))
        {
            _parser.Advance();
            _parser.Advance();
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
}
