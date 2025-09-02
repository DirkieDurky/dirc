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
        Token baseTypeToken = _context.ParserBase.Consume(TokenType.Identifier, "Expected type name");
        TypeNode type = new NamedTypeNode(baseTypeToken, baseTypeToken.Lexeme);

        while (_context.ParserBase.Match(TokenType.Asterisk))
        {
            type = new PointerTypeNode(baseTypeToken, type);
        }

        return type;
    }
}
