using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

/// <summary>
/// Manages operator precedence and provides operator-related utilities
/// </summary>
class OperatorPrecedence
{
    private readonly Dictionary<int, HashSet<TokenType>> _precedenceLevels;
    private readonly Dictionary<TokenType, Operation> _operations;

    public OperatorPrecedence()
    {
        // Setup precedence levels from lowest to highest
        _precedenceLevels = new Dictionary<int, HashSet<TokenType>>
        {
            // Level 0: Comparison operators
            { 0, new HashSet<TokenType>
                {
                    TokenType.EqualEqual, TokenType.NotEqual,
                    TokenType.Less, TokenType.LessEqual,
                    TokenType.Greater, TokenType.GreaterEqual
                }
            },

            // Level 1: Bitwise OR
            { 1, new HashSet<TokenType> { TokenType.Pipe } },
            
            // Level 2: Bitwise XOR
            { 2, new HashSet<TokenType> { TokenType.Caret } },
            
            // Level 3: Bitwise AND
            { 3, new HashSet<TokenType> { TokenType.Ampersand } },
            
            // Level 4: Addition/Subtraction
            { 4, new HashSet<TokenType> { TokenType.Plus, TokenType.Minus } },
            
            // Level 5: Multiplication/Division/Modulo
            { 5, new HashSet<TokenType> { TokenType.Asterisk, TokenType.Slash, TokenType.Percent } }
        };

        // Map tokens to operations
        _operations = new Dictionary<TokenType, Operation>
        {
            // Arithmetic operators
            { TokenType.Plus, Operation.Add },
            { TokenType.Minus, Operation.Sub },
            { TokenType.Asterisk, Operation.Mul },
            { TokenType.Slash, Operation.Div },
            { TokenType.Percent, Operation.Mod },
            
            // Bitwise operators
            { TokenType.Pipe, Operation.Or },
            { TokenType.Ampersand, Operation.And },
            { TokenType.Caret, Operation.Xor },
            
            // Comparison operators
            { TokenType.EqualEqual, Operation.Equal },
            { TokenType.NotEqual, Operation.NotEqual },
            { TokenType.Less, Operation.LessThan },
            { TokenType.LessEqual, Operation.LessEqual },
            { TokenType.Greater, Operation.GreaterThan },
            { TokenType.GreaterEqual, Operation.GreaterEqual }
        };
    }

    public int MaxLevel => _precedenceLevels.Count;

    public bool IsOperatorAtLevel(int level, TokenType type)
    {
        return _precedenceLevels.ContainsKey(level) && _precedenceLevels[level].Contains(type);
    }

    public Operation GetOperation(TokenType type)
    {
        if (!_operations.ContainsKey(type))
            throw new InvalidOperationException($"Unknown operator: {type}");

        return _operations[type];
    }

    public bool IsValidOperation(TokenType type) => _operations.ContainsKey(type);
}
