using System.Text.RegularExpressions;
using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Handles parsing of function declarations and parameters
/// </summary>
internal class FunctionParser
{
    private readonly ParserContext _context;

    public FunctionParser(ParserContext context)
    {
        _context = context;
    }

    public FunctionDeclarationNode ParseFunctionDeclaration(TypeNode returnType)
    {
        Token name = _context.ParserBase.Advance();
        _context.ParserBase.Consume(TokenType.LeftParen, "Expected '(' after function name");

        List<FunctionParameterNode> parameters = ParseFunctionParameters();
        _context.ParserBase.Consume(TokenType.RightParen, "Expected ')' after parameters");

        return new FunctionDeclarationNode(name, name.Lexeme, returnType, parameters, ParseBody("function"));
    }

    private List<FunctionParameterNode> ParseFunctionParameters()
    {
        List<FunctionParameterNode> parameters = new();
        if (!_context.ParserBase.Check(TokenType.RightParen))
        {
            do
            {
                TypeNode paramType = _context.TypeParser.ParseType();
                Token paramName = _context.ParserBase.Consume(TokenType.Identifier, "No parameter name provided");

                // Array parameters
                while (_context.ParserBase.Match(TokenType.LeftBracket))
                {
                    paramType = new PointerTypeNode(paramType.IdentifierToken, paramType, true);
                    _context.ParserBase.Consume(TokenType.RightBracket, "Expected closing bracket after opening bracket");
                }

                parameters.Add(new FunctionParameterNode(paramName, paramType, paramName.Lexeme));
            } while (_context.ParserBase.Match(TokenType.Comma));
        }
        return parameters;
    }

    private List<AstNode> ParseBody(string kind)
    {
        _context.ParserBase.Consume(TokenType.LeftBrace, $"Expected '{{' after {kind}");
        List<AstNode> body = new();
        while (!_context.ParserBase.Check(TokenType.RightBrace) && !_context.ParserBase.IsAtEnd())
        {
            body.AddRange(_context.StatementParser.ParseStatement());
        }
        _context.ParserBase.Consume(TokenType.RightBrace, $"Expected '}}' after {kind} body");
        return body;
    }
}
