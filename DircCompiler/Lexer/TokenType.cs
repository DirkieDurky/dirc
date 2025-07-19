namespace DircCompiler.Lexing;

public enum TokenType
{
    // Single-character symbols
    LeftParen, RightParen,
    LeftBrace, RightBrace,
    Comma, Semicolon,
    Plus, Minus, Asterisk, Slash, Or, And, Xor,
    Equals, ExclamationPoint,

    // Multi-character operators
    // Conditions
    EqualEqual, NotEqual,
    Less, LessEqual,
    Greater, GreaterEqual,
    // Assignment shorthands
    PlusEqual, MinusEqual, AsteriskEqual,
    SlashEqual, OrEqual, AndEqual, XorEqual,

    // Literals
    Identifier,
    Number,
    BinaryNumber,
    HexNumber,
    True,
    False,

    // Keywords
    Import,
    If, Else,
    Return,
    While,
    For,
}
