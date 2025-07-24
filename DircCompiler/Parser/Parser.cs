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
            statements.AddRange(ParseStatement());
        }
        return statements;
    }

    private List<AstNode> ParseStatement()
    {
        if (Match(TokenType.Return))
        {
            ReturnStatementNode node = new(ParseExpression());
            Consume(TokenType.Semicolon, "Expected ';' after expression");
            return [node];
        }
        if (Match(TokenType.Import))
        {
            Token name = Consume(TokenType.Identifier, "No import function name provided");
            Consume(TokenType.Semicolon, "Expected ';' after import");
            return [new ImportStatementNode(name, name.Lexeme)];
        }

        // Function, array, or variable declarations
        if (Check(TokenType.Identifier) && CheckNext(TokenType.Identifier))
        {
            Token typeToken = Advance();
            Token nameToken = Advance();
            if (Match(TokenType.LeftParen))
            {
                AstNode node = ParseFunctionDeclaration(nameToken, typeToken);
                return [node];
            }
            else if (Match(TokenType.LeftBracket))
            {
                AstNode node = ParseArrayDeclaration(typeToken, nameToken);
                Consume(TokenType.Semicolon, "Expected ';' after array declaration");
                return [node];
            }
            else
            {
                VariableDeclarationNode node = ParseVariableDeclaration(typeToken, nameToken);
                Consume(TokenType.Semicolon, "Expected ';' after variable declaration");
                return [node];
            }
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

            return [new IfStatementNode(condition, body, elseBody)];
        }
        else if (Match(TokenType.While))
        {
            Consume(TokenType.LeftParen, "Expected '(' after while keyword");
            AstNode condition = ParseCondition();
            Consume(TokenType.RightParen, "Expected ')' after while condition");
            List<AstNode> body = ParseBody("while statement");

            return [new WhileStatementNode(condition, body)];
        }
        else if (Match(TokenType.For))
        {
            Consume(TokenType.LeftParen, "Expected '(' after for keyword");

            AstNode? initial = null;
            if (!Match(TokenType.Semicolon))
            {
                if (Check(TokenType.Identifier) && CheckNext(TokenType.Identifier))
                {
                    Token typeToken = Advance();
                    Token nameToken = Advance();
                    initial = ParseVariableDeclaration(typeToken, nameToken);
                }
                else
                {
                    initial = ParseExpression();
                }
                Consume(TokenType.Semicolon, "Expected ';' after for expression 1");
            }

            AstNode? condition = null;
            if (!Match(TokenType.Semicolon))
            {
                condition = ParseCondition();
                Consume(TokenType.Semicolon, "Expected ';' after for expression 2");
            }

            AstNode? increment = null;
            if (!Match(TokenType.RightParen))
            {
                increment = ParseExpression();
                Consume(TokenType.RightParen, "Expected ')' after for expression 3");
            }

            List<AstNode> body = ParseBody("for statement");

            if (increment != null) body.Add(increment);
            List<AstNode> result = [];
            if (initial != null) result.Add(initial);
            result.Add(new WhileStatementNode(condition ?? new BooleanLiteralNode(true), body));
            return result;
        }
        else if (Check(TokenType.Identifier))
        {
            if (CheckNext(TokenType.LeftBracket))
            {
                // Array access or assignment
                AstNode node = ParsePrimary();
                Consume(TokenType.Semicolon, "Expected ';' after array statement");
                return [node];
            }
            else
            {
                // Variable assignment or function call
                Token name = Advance();
                if (Match(TokenType.LeftParen))
                {
                    AstNode node = ParseCall(name);
                    Consume(TokenType.Semicolon, "Expected ';' after function call");
                    return [node];
                }
                else
                {
                    VariableAssignmentNode node = ParseVariableAssignment(name);
                    Consume(TokenType.Semicolon, "Expected ';' after variable assignment");
                    return [node];
                }
            }
        }
        while (Match(TokenType.Semicolon)) { }
        return [ParseExpressionStatement()];
    }

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

    private AstNode ParseCall(Token nameToken)
    {
        List<AstNode> args = new();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                args.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expected ')' after arguments");
        return new CallExpressionNode(nameToken, nameToken.Lexeme, args);
    }

    private FunctionDeclarationNode ParseFunctionDeclaration(Token nameToken, Token returnTypeToken)
    {
        List<FunctionParameterNode> parameters = new();
        if (!Check(TokenType.RightParen))
        {
            do
            {
                Token paramType = Consume(TokenType.Identifier, "No parameter type provided");
                Token paramName = Consume(TokenType.Identifier, "No parameter name provided");
                parameters.Add(new FunctionParameterNode(paramType, new NamedTypeNode(paramType, paramType.Lexeme), paramName.Lexeme));
            } while (Match(TokenType.Comma));
        }
        Consume(TokenType.RightParen, "Expected ')' after parameters");
        return new FunctionDeclarationNode(nameToken, nameToken.Lexeme, new NamedTypeNode(returnTypeToken, returnTypeToken.Lexeme), parameters, ParseBody("function"));
    }

    private VariableDeclarationNode ParseVariableDeclaration(Token typeToken, Token nameToken)
    {
        AstNode? initializer = null;
        if (Match(TokenType.Equals))
        {
            initializer = ParseExpression();
        }
        return new VariableDeclarationNode(new NamedTypeNode(typeToken, typeToken.Lexeme), nameToken, initializer);
    }

    private ArrayDeclarationNode ParseArrayDeclaration(Token typeToken, Token nameToken)
    {
        AstNode size = ParseExpression();
        Consume(TokenType.RightBracket, "Expected ']' after array size");

        AstNode? initializer = null;
        if (Match(TokenType.Equals))
        {
            initializer = ParseArrayLiteral();
        }

        return new ArrayDeclarationNode(new NamedTypeNode(typeToken, typeToken.Lexeme), nameToken, size, initializer);
    }

    private ArrayLiteralNode ParseArrayLiteral()
    {
        Consume(TokenType.LeftBrace, "Expected '{' at start of array literal");
        List<AstNode> elements = new();

        if (!Check(TokenType.RightBrace))
        {
            do
            {
                elements.Add(ParseExpression());
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.RightBrace, "Expected '}' at end of array literal");
        return new ArrayLiteralNode(elements);
    }

    private List<AstNode> ParseBody(string kind)
    {
        Consume(TokenType.LeftBrace, $$"""Expected '{' after {{kind}}""");
        List<AstNode> body = new();
        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            body.AddRange(ParseStatement());
        }
        Consume(TokenType.RightBrace, $$"""Expected '}' after {{kind}} body""");

        return body;
    }

    private VariableAssignmentNode ParseVariableAssignment(Token name)
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
                return new VariableAssignmentNode(name, name.Lexeme, valuePlusOne);
            }

            if (Check(TokenType.Minus) && CheckNext(TokenType.Minus))
            {
                Advance();
                Advance();
                BinaryExpressionNode valuePlusOne = new BinaryExpressionNode(Operation.Sub, new IdentifierNode(name, name.Lexeme), new NumberLiteralNode(1));
                return new VariableAssignmentNode(name, name.Lexeme, valuePlusOne);
            }

            Operation op = OperationFromToken(Advance());
            Consume(TokenType.Equals, "Expected '=' after operation in variable assignment shorthand");

            value = ParseExpression();

            BinaryExpressionNode newValue = new BinaryExpressionNode(op, new IdentifierNode(name, name.Lexeme), value);
            return new VariableAssignmentNode(name, name.Lexeme, newValue);
        }

        return new VariableAssignmentNode(name, name.Lexeme, value);
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

    // Done this way to preserve operator precedence:
    // Or | Xor | And | Addition | Multiplication | Primary
    private AstNode ParseOr()
    {
        AstNode expr = ParseXor();
        while (Match(TokenType.Pipe))
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
        while (Match(TokenType.Caret))
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
        while (Match(TokenType.Ampersand))
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
            return new BooleanLiteralNode(true);
        }
        if (Match(TokenType.False))
        {
            return new BooleanLiteralNode(false);
        }

        if (Match(TokenType.Identifier))
        {
            Token name = Previous();
            if (Match(TokenType.LeftParen))
            {
                return ParseCall(name);
            }
            else if (Match(TokenType.LeftBracket))
            {
                AstNode index = ParseExpression();
                Consume(TokenType.RightBracket, "Expected ']' after array index");

                if (Match(TokenType.Equals))
                {
                    AstNode value = ParseExpression();
                    return new ArrayAssignmentNode(name, index, value);
                }
                else
                {
                    return new ArrayAccessNode(name, index);
                }
            }
            else
            {
                if (Check(TokenType.Equals) || CheckNext(TokenType.Equals)
                || (Check(TokenType.Plus) && CheckNext(TokenType.Plus)))
                {
                    return ParseVariableAssignment(name);
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
            TokenType.Pipe => Operation.Or,
            TokenType.Ampersand => Operation.And,
            TokenType.Caret => Operation.Xor,
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
