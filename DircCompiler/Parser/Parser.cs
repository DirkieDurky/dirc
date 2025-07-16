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
        if (Match(TokenType.Return))
        {
            ReturnStatementNode node = new(ParseExpression());
            Consume(TokenType.Semicolon, "Expected ';' after expression");
            return node;
        }
        if (Match(TokenType.Import))
        {
            Token name = Consume(TokenType.Identifier, "No import function name provided");
            Consume(TokenType.Semicolon, "Expected ';' after import");
            return new ImportStatementNode(name, name.Lexeme);
        }
        if (Match(TokenType.Function))
        {
            Token name = Consume(TokenType.Identifier, "No function name provided");
            Consume(TokenType.LeftParen, "Expected '(' after function name");
            AstNode node = ParseFunction(name, true);
            return node;
        }
        else if (Match(TokenType.Var))
        {
            Token name = Consume(TokenType.Identifier, "No variable name provided");
            VariableAssignmentNode node = ParseVariableAssignment(name, true);
            Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
            return node;
        }
        else if (Match(TokenType.If))
        {
            Consume(TokenType.LeftParen, "Expected '(' after if keyword");

            AstNode condition = ParseCondition();
            Consume(TokenType.RightParen, "Expected ')' after if condition");
            List<AstNode> body = ParseBody("if statement");
            List<AstNode>? elseBody = null;
            if (Match(TokenType.Else))
            {
                elseBody = ParseBody("else statement");
            }

            return new IfStatementNode(condition, body, elseBody);
        }
        else if (Match(TokenType.Identifier))
        {
            Token name = Previous();
            if (Match(TokenType.LeftParen))
            {
                AstNode node = ParseFunction(name);
                if (node is CallExpressionNode)
                {
                    Consume(TokenType.Semicolon, "Expected ';' after function call");
                }
                return node;
            }
            else
            {
                VariableAssignmentNode node = ParseVariableAssignment(name, false);
                Consume(TokenType.Semicolon, "Expected ';' after variable assignment or declaration");
                return node;
            }
        }
        return ParseExpressionStatement();
    }

    // Add this new method for comparison expressions
    private AstNode ParseCondition()
    {
        AstNode expr = ParseOr();
        while (Check(TokenType.EqualEqual) || Check(TokenType.NotEqual) ||
               Check(TokenType.Less) || Check(TokenType.LessEqual) ||
               Check(TokenType.Greater) || Check(TokenType.GreaterEqual))
        {
            Token opToken = Advance();
            Comparer comparer = opToken.Type switch
            {
                TokenType.EqualEqual => Comparer.IfEq,
                TokenType.NotEqual => Comparer.IfNotEq,
                TokenType.Less => Comparer.IfLess,
                TokenType.LessEqual => Comparer.IfLessOrEq,
                TokenType.Greater => Comparer.IfMore,
                TokenType.GreaterEqual => Comparer.IfMoreOrEq,
                _ => throw new SyntaxException("Invalid comparer specified", opToken, _compilerOptions, _compilerContext)
            };
            AstNode right = ParseOr();
            expr = new ConditionNode(comparer, expr, right);
        }
        return expr;
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

        if (Check(TokenType.LeftBrace) || isDeclaration)
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
            return new CallExpressionNode(name, name.Lexeme, parametersOrArguments);
        }
    }

    private FunctionDeclarationNode ParseFunctionDeclaration(Token name, List<string> parameters)
    {
        return new FunctionDeclarationNode(name, name.Lexeme, parameters, ParseBody("function"));
    }

    private List<AstNode> ParseBody(string kind)
    {
        Consume(TokenType.LeftBrace, $$"""Expected '{' after {{kind}}""");
        List<AstNode> body = new();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            body.Add(ParseStatement());
        }
        Consume(TokenType.RightBrace, $$"""Expected '}' after {{kind}} body""");

        return body;
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
                BinaryExpressionNode valuePlusOne = new BinaryExpressionNode(Operation.Add, new IdentifierNode(name, name.Lexeme), new NumberLiteralNode(1));

                return new VariableAssignmentNode(name, name.Lexeme, false, true, valuePlusOne);
            }

            if (Check(TokenType.Minus) && CheckNext(TokenType.Minus))
            {
                Advance();
                Advance();
                BinaryExpressionNode valuePlusOne = new BinaryExpressionNode(Operation.Sub, new IdentifierNode(name, name.Lexeme), new NumberLiteralNode(1));

                return new VariableAssignmentNode(name, name.Lexeme, false, true, valuePlusOne);
            }

            Operation op = OperationFromToken(Advance());
            Consume(TokenType.Equals, "Expected '=' after operation in variable assignment shorthand");

            value = ParseExpression();

            BinaryExpressionNode newValue = new BinaryExpressionNode(op, new IdentifierNode(name, name.Lexeme), value);

            return new VariableAssignmentNode(name, name.Lexeme, false, true, newValue);
        }

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
        return ParseCondition();
    }

    // Operator precedence: Or | Xor | And | Addition | Multiplication | Primary
    private AstNode ParseOr()
    {
        AstNode expr = ParseXor();
        while (Match(TokenType.Or))
        {
            Operation op = OperationFromToken(Previous());
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
            Operation op = OperationFromToken(Previous());
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
            Operation op = OperationFromToken(Previous());
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
            Operation op = OperationFromToken(Previous());
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
            Operation op = OperationFromToken(Previous());
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

        if (Match(TokenType.True))
        {
            return new NumberLiteralNode(NumberLiteralType.Decimal, "1");
        }
        if (Match(TokenType.False))
        {
            return new NumberLiteralNode(NumberLiteralType.Decimal, "0");
        }

        if (Match(TokenType.Identifier))
        {
            Token name = Previous();
            if (Match(TokenType.LeftParen))
            {
                return ParseFunction(name);
            }
            else
            {
                if (Check(TokenType.Equals) || CheckNext(TokenType.Equals)
                || (Check(TokenType.Plus) && CheckNext(TokenType.Plus)))
                {
                    return ParseVariableAssignment(name, false);
                }
                else
                {
                    return new IdentifierNode(Previous(), Previous().Lexeme);
                }
            }
        }

        if (IsAtEnd()) throw new SyntaxException($"Unexpected end of text", Previous(), _compilerOptions, _compilerContext);
        throw new SyntaxException($"Unexpected token", Previous(), _compilerOptions, _compilerContext);
    }

    // Utility methods
    private Operation OperationFromToken(Token token)
    {
        return token.Type switch
        {
            TokenType.Plus => Operation.Add,
            TokenType.Minus => Operation.Sub,
            TokenType.Asterisk => Operation.Mul,
            TokenType.Slash => Operation.Div,
            TokenType.Or => Operation.Or,
            TokenType.And => Operation.And,
            TokenType.Xor => Operation.Xor,
            _ => throw new SyntaxException("Invalid operation specified", token, _compilerOptions, _compilerContext)
        };
    }
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
