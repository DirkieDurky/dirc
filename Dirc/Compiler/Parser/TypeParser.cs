using System.Collections.Concurrent;
using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of type declarations including pointer types
/// </summary>
class TypeParser
{
    private readonly ParserContext _context;

    public TypeParser(ParserContext context)
    {
        _context = context;
    }

    public TypeNode ParseType()
    {
        TryParseType(out TypeNode? type, false);
        return type!;
    }

    public bool TryParseType(out TypeNode? type, bool safe = true) // safe: if true, checks before everything that can fail
    {
        type = null;
        _context.ParserBase.SetCheckpoint();

        if (safe && _context.ParserBase.Peek().Type != TokenType.Identifier)
        {
            _context.ParserBase.ReturnToCheckpoint();
            return false;
        }
        Token baseTypeToken = _context.ParserBase.Consume(TokenType.Identifier, "Expected type name");
        type = new NamedTypeNode(baseTypeToken, baseTypeToken.Lexeme);
        List<int> arraySizes = [];

        while (_context.ParserBase.Match(TokenType.Asterisk) || _context.ParserBase.Match(TokenType.LeftBracket))
        {
            bool previousWasBracket = _context.ParserBase.Previous().Type == TokenType.LeftBracket;
            type = new PointerTypeNode(baseTypeToken, type, previousWasBracket);
            if (previousWasBracket)
            {
                if (_context.ParserBase.Peek().Type != TokenType.RightBracket)
                {
                    AstNode sizeNode = _context.ExpressionParser.ParseExpression();
                    if (sizeNode is not NumberLiteralNode sizeNum || sizeNum.Value.Any(c => !char.IsDigit(c)))
                    {
                        _context.ParserBase.ReturnToCheckpoint();
                        return false;
                    }

                    arraySizes.Add(int.Parse(sizeNum.Value));
                }

                if (safe && _context.ParserBase.Peek().Type != TokenType.RightBracket)
                {
                    _context.ParserBase.ReturnToCheckpoint();
                    return false;
                }
                _context.ParserBase.Consume(TokenType.RightBracket, "Expected closing bracket after opening bracket");
            }
        }

        if (safe
        && !_context.ParserBase.Check(TokenType.Identifier)
        && !_context.ParserBase.CheckNext(TokenType.LeftBracket) // This would mean array type like char[] so char should be treated as a type
        )
        {
            _context.ParserBase.ReturnToCheckpoint();
            return false;
        }

        if (safe && _context.ParserBase.CheckNext(TokenType.Identifier))
        {
            _context.ParserBase.ReturnToCheckpoint();
            return false;
        }

        type.ArraySizes.AddRange(arraySizes);

        return true;
    }
}
