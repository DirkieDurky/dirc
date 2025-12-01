using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of variable declarations and assignments
/// </summary>
internal class VariableParser
{
    private readonly ParserContext _context;

    public VariableParser(ParserContext context)
    {
        _context = context;
    }

    public VariableDeclarationNode ParseVariableDeclaration(TypeNode? type = null)
    {
        type ??= _context.TypeParser.ParseType();
        Token name = _context.ParserBase.Advance();

        AstNode? initializer = null;
        if (_context.ParserBase.Match(TokenType.Equals))
            initializer = _context.ExpressionParser.ParseExpression();

        return new VariableDeclarationNode(type, name, initializer);
    }

    internal VariableAssignmentNode ParseVariableAssignment()
    {
        AstNode target;
        Token? targetName = null;

        var pointerDereference = _context.PointerParser.ParsePointerDereference();
        if (pointerDereference != null)
        {
            target = pointerDereference;
        }
        else
        {
            Token identifierToken = _context.ParserBase.Consume(TokenType.Identifier, "Expected identifier in variable assignment");
            target = new IdentifierNode(identifierToken, identifierToken.Lexeme);
            targetName = identifierToken;
        }

        if (_context.ParserBase.Match(TokenType.Equals))
        {
            AstNode assignmentValue = _context.ExpressionParser.ParseExpression();
            return new VariableAssignmentNode(target, targetName, assignmentValue);
        }

        if (_context.ParserBase.Check(TokenType.Plus) && _context.ParserBase.CheckNext(TokenType.Plus))
        {
            _context.ParserBase.Advance();
            _context.ParserBase.Advance();
            return new VariableAssignmentNode(target, targetName,
                new BinaryExpressionNode(Operation.Add, target, new NumberLiteralNode(1)));
        }

        if (_context.ParserBase.Check(TokenType.Minus) && _context.ParserBase.CheckNext(TokenType.Minus))
        {
            _context.ParserBase.Advance();
            _context.ParserBase.Advance();
            return new VariableAssignmentNode(target, targetName,
                new BinaryExpressionNode(Operation.Sub, target, new NumberLiteralNode(1)));
        }

        Operation op = GetOperation(_context.ParserBase.Advance());
        _context.ParserBase.Consume(TokenType.Equals, "Expected '=' after operation in variable assignment");

        AstNode value = _context.ExpressionParser.ParseExpression();
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
            { TokenType.Percent, Operation.Mod },

            { TokenType.BitshiftLeft, Operation.BitshiftLeft },
            { TokenType.BitshiftRight, Operation.BitshiftRight },
            { TokenType.Pipe, Operation.Or },
            { TokenType.Ampersand, Operation.And },
            { TokenType.Caret, Operation.Xor },
        };

        if (!operations.ContainsKey(token.Type))
            throw new SyntaxException("Invalid operation specified", token, _context.ParserBase.Options, _context.ParserBase.Context);

        return operations[token.Type];
    }
}
