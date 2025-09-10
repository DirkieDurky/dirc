using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of array declarations and literals
/// </summary>
internal class ArrayParser
{
    private readonly ParserContext _context;

    public ArrayParser(ParserContext context)
    {
        _context = context;
    }

    public ArrayDeclarationNode ParseArrayDeclaration(TypeNode type)
    {
        Token name = _context.ParserBase.Advance();

        _context.ParserBase.Consume(TokenType.LeftBracket, "Expected '[' in array declaration");
        AstNode sizeNode = _context.ExpressionParser.ParseExpression();
        if (sizeNode is not NumberLiteralNode sizeNum)
        {
            throw new SyntaxException("Size in array declaration must be a number", name, _context.ParserBase.Options, _context.ParserBase.Context);
        }
        _context.ParserBase.Consume(TokenType.RightBracket, "Expected ']' after array size");

        AstNode? initializer = null;
        if (_context.ParserBase.Match(TokenType.Equals))
            initializer = _context.ParserBase.Match(TokenType.String) ? new StringLiteralNode(_context.ParserBase.Previous()) : ParseArrayLiteral();

        return new ArrayDeclarationNode(type, name, int.Parse(sizeNum.Value), initializer);
    }

    public ArrayLiteralNode ParseArrayLiteral()
    {
        _context.ParserBase.Consume(TokenType.LeftBrace, "Expected '{' at start of array literal");
        List<AstNode> elements = new();

        if (!_context.ParserBase.Check(TokenType.RightBrace))
        {
            do
            {
                elements.Add(_context.ExpressionParser.ParseExpression());
            } while (_context.ParserBase.Match(TokenType.Comma));
        }

        _context.ParserBase.Consume(TokenType.RightBrace, "Expected '}' at end of array literal");
        return new ArrayLiteralNode(elements);
    }
}
