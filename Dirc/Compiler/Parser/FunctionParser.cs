using System.Text.RegularExpressions;
using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of function declarations and parameters
/// </summary>
internal class FunctionParser
{
    private readonly ParserBase _parser;
    private readonly TypeParser _typeParser;
    private readonly StatementParser _statementParser;

    public FunctionParser(ParserBase parser, StatementParser statementParser)
    {
        _parser = parser;
        _typeParser = new TypeParser(parser);
        _statementParser = statementParser;
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

                // Array parameters
                if (_parser.Match(TokenType.LeftBracket))
                {
                    paramType = new PointerTypeNode(paramType.IdentifierToken, paramType);
                    _parser.Consume(TokenType.RightBracket, "Expected closing bracket after opening bracket");
                }

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
            body.AddRange(_statementParser.ParseStatement());
        }
        _parser.Consume(TokenType.RightBrace, $"Expected '}}' after {kind} body");
        return body;
    }
}
