using System.Runtime.CompilerServices;
using DircCompiler.Lexing;

namespace DircCompiler.Parsing;

class Parser
{
    private List<Token> _tokens = new();
    private int _current;
    private CompilerContext _compilerContext;
    private CompilerOptions _compilerOptions;

    public Parser(CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        _compilerContext = compilerContext;
        _compilerOptions = compilerOptions;
    }

    public List<AstNode> Parse(List<Token> tokens)
    {
        _tokens = tokens;
        _current = 0;
        List<AstNode> statements = new List<AstNode>();
        while (!IsAtEnd())
        {
            statements.Add(ParseStatement());
        }
        return statements;
    }

    private AstNode ParseStatement()
    {
        if (Match(TokenType.Function))
        {
            Token name = Consume(TokenType.Identifier, "No function name provided");
            Consume(TokenType.LeftParen, "Expected '(' after function name");
            return ParseFunction(name, true);
        }
        else if (Match(TokenType.Var))
        {
            Token name = Consume(TokenType.Identifier, "No variable name provided");
            return ParseVariableAssignment(name, true);
        }
        else if (Match(TokenType.Identifier))
        {
            Token name = Previous();
            if (Match(TokenType.LeftParen))
            {
                return ParseFunction(name);
            }
            else
            {
                return ParseVariableAssignment(name, false);
            }
        }
        return ParseExpressionStatement();
    }

    // If isDeclaration is true we know it's a function declaration
    // Otherwise we figure it out based on the presence of a LeftBrace.
    private AstNode ParseFunction(Token name, bool isDeclaration = false)
    {
        List<AstNode> parametersOrArguments = new();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                parametersOrArguments.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expected ')' after parameters");

        if (Match(TokenType.LeftBrace) || isDeclaration)
        {
            List<string> parameters = new();
            foreach (AstNode node in parametersOrArguments)
            {
                if (node is IdentifierNode parameter) parameters.Add(parameter.Name);
                else throw new SyntaxException("Function parameters containing expression", Peek(), _compilerOptions, _compilerContext);
            }
            return ParseFunctionDeclaration(name, parameters);
        }
        else
        {
            Consume(TokenType.Semicolon, "Expected ';' after function call");
            return new CallExpressionNode(name, name.Lexeme, parametersOrArguments);
        }
    }

    private FunctionDeclarationNode ParseFunctionDeclaration(Token name, List<string> parameters)
    {
        List<AstNode> body = new List<AstNode>();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            body.Add(ParseStatement());
        }
        Consume(TokenType.RightBrace, "Expected '}' after function body");
        return new FunctionDeclarationNode(name, name.Lexeme, parameters, body);
    }

    private VariableAssignmentNode ParseVariableAssignment(Token name, bool isDeclaration)
    {
        AstNode? value;
        if (Match(TokenType.Equals))
        {
            value = ParseExpression();
        }
        else
        {
            if (Check(TokenType.Plus) && CheckNext(TokenType.Plus))
            {
                Advance();
                Advance();
                BinaryExpressionNode valuePlusOne = new BinaryExpressionNode("+", new IdentifierNode(name, name.Lexeme), new NumberLiteralNode(1));
                Consume(TokenType.Semicolon, "Expected ';' after variable assignment or declaration");

                return new VariableAssignmentNode(name, name.Lexeme, false, true, valuePlusOne);
            }

            if (Check(TokenType.Minus) && CheckNext(TokenType.Minus))
            {
                Advance();
                Advance();
                BinaryExpressionNode valuePlusOne = new BinaryExpressionNode("-", new IdentifierNode(name, name.Lexeme), new NumberLiteralNode(1));
                Consume(TokenType.Semicolon, "Expected ';' after variable assignment or declaration");

                return new VariableAssignmentNode(name, name.Lexeme, false, true, valuePlusOne);
            }

            string operation = Advance().Type switch
            {
                TokenType.Plus => "+",
                TokenType.Minus => "-",
                TokenType.Asterisk => "*",
                TokenType.Slash => "/",
                TokenType.Or => "|",
                TokenType.And => "&",
                TokenType.Xor => "^",
                _ => throw new SyntaxException("Invalid statement", Peek(), _compilerOptions, _compilerContext)
            };
            Consume(TokenType.Equals, "Expected '=' after operation in variable assignment shorthand");

            value = ParseExpression();

            BinaryExpressionNode newValue = new BinaryExpressionNode(operation, new IdentifierNode(name, name.Lexeme), value);
            Consume(TokenType.Semicolon, "Expected ';' after variable assignment or declaration");

            return new VariableAssignmentNode(name, name.Lexeme, false, true, newValue);
        }

        Consume(TokenType.Semicolon, "Expected ';' after variable assignment or declaration");
        return new VariableAssignmentNode(name, name.Lexeme, isDeclaration, false, value);
    }

    private ExpressionStatementNode ParseExpressionStatement()
    {
        AstNode expr = ParseExpression();
        Consume(TokenType.Semicolon, "Expected ';' after expression");
        return new ExpressionStatementNode(expr);
    }

    private AstNode ParseExpression()
    {
        return ParseOr();
    }

    // Operator precedence: Or | Xor | And | Addition | Multiplication | Primary
    private AstNode ParseOr()
    {
        AstNode expr = ParseXor();
        while (Match(TokenType.Or))
        {
            string op = Previous().Lexeme;
            AstNode right = ParseXor();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseXor()
    {
        AstNode expr = ParseAnd();
        while (Match(TokenType.Xor))
        {
            string op = Previous().Lexeme;
            AstNode right = ParseAnd();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseAnd()
    {
        AstNode expr = ParseAddition();
        while (Match(TokenType.And))
        {
            string op = Previous().Lexeme;
            AstNode right = ParseAddition();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseAddition()
    {
        AstNode expr = ParseMultiplication();
        while (Match(TokenType.Plus) || Match(TokenType.Minus))
        {
            string op = Previous().Lexeme;
            AstNode right = ParseMultiplication();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParseMultiplication()
    {
        AstNode expr = ParsePrimary();
        while (Match(TokenType.Asterisk) || Match(TokenType.Slash))
        {
            string op = Previous().Lexeme;
            AstNode right = ParsePrimary();
            expr = new BinaryExpressionNode(op, expr, right);
        }
        return expr;
    }

    private AstNode ParsePrimary()
    {
        if (Match(TokenType.Number))
        {
            return new NumberLiteralNode(NumberLiteralType.Decimal, (string)Previous().Literal!);
        }

        if (Match(TokenType.BinaryNumber))
        {
            return new NumberLiteralNode(NumberLiteralType.Binary, (string)Previous().Literal!);
        }

        if (Match(TokenType.HexNumber))
        {
            return new NumberLiteralNode(NumberLiteralType.Hexadecimal, (string)Previous().Literal!);
        }

        if (Match(TokenType.Identifier))
        {
            return new IdentifierNode(Previous(), Previous().Lexeme);
        }
        if (IsAtEnd()) throw new SyntaxException($"Unexpected end of text", Previous(), _compilerOptions, _compilerContext);
        throw new SyntaxException($"Unexpected token", Previous(), _compilerOptions, _compilerContext);
    }

    // Utility methods
    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private bool CheckNext(TokenType type)
    {
        if (_current + 1 >= _tokens.Count) return false;
        return _tokens[_current + 1].Type == type;
    }

    private Token Advance()
    {
        return _tokens[_current++];
    }

    private bool IsAtEnd() => _current >= _tokens.Count;
    private Token Peek() => _tokens[_current];
    private Token Previous() => _tokens[_current - 1];

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw new SyntaxException(message, Previous(), _compilerOptions, _compilerContext);
    }
}
