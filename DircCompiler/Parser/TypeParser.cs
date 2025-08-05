using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Handles parsing of type declarations including pointer types
/// </summary>
internal class TypeParser
{
    private readonly ParserBase _parser;

    public TypeParser(ParserBase parser)
    {
        _parser = parser;
    }

    public TypeNode ParseType()
    {
        Token baseTypeToken = _parser.Consume(TokenType.Identifier, "Expected type name");
        TypeNode type = new NamedTypeNode(baseTypeToken, baseTypeToken.Lexeme);

        while (_parser.Match(TokenType.Asterisk))
        {
            type = new PointerTypeNode(baseTypeToken, type);
        }

        return type;
    }
}
