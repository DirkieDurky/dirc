public enum TokenType
{
    // Single-character symbols
    LeftParen, RightParen,
    LeftBrace, RightBrace,
    Comma, Semicolon,
    Plus, Minus, Asterisk, Slash, Or, And, Xor,
    Equals, ExclamationPoint,

    // Multi-character operators
    EqualEqual, NotEqual,
    Less, LessEqual,
    Greater, GreaterEqual,

    // Literals
    Identifier,
    Number,
    BinaryNumber,
    HexNumber,

    // Keywords
    Function,
    Var,
}
