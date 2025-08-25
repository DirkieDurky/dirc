using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

/// <summary>
/// Handles parsing of array declarations and literals
/// </summary>
internal class ArrayParser
{
    private readonly ParserBase _parser;
    private readonly TypeParser _typeParser;
    private readonly ExpressionParser _expressionParser;

    public ArrayParser(ParserBase parser)
    {
        _parser = parser;
        _typeParser = new TypeParser(parser);
        _expressionParser = new ExpressionParser(parser);
    }

    public ArrayDeclarationNode ParseArrayDeclaration()
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
}
