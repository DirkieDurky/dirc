using DircCompiler.Lexing;

namespace DircCompiler.Parsing.Factories;

internal class FunctionFactory
{
    private readonly ParserBase _parser;
    private readonly TypeParser _typeParser;
    private readonly ExpressionParser _expressionParser;

    public FunctionFactory(ParserBase parser, TypeParser typeParser, ExpressionParser expressionParser)
    {
        _parser = parser;
        _typeParser = typeParser;
        _expressionParser = expressionParser;
    }

    public FunctionDeclarationNode ParseFunctionDeclaration()
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

    private List<AstNode> ParseBody(string kind)
    {
        _parser.Consume(TokenType.LeftBrace, $"Expected '{{' after {kind}");
        List<AstNode> body = new();
        while (!_parser.Check(TokenType.RightBrace) && !_parser.IsAtEnd())
        {
            body.AddRange(new StatementParser(_parser).ParseStatement());
        }
        _parser.Consume(TokenType.RightBrace, $"Expected '}}' after {kind} body");
        return body;
    }
}
