namespace Dirc.Compiling.Lexing;

public enum TokenType
{
    // Single-character symbols
    LeftParen, RightParen,
    LeftBrace, RightBrace,
    LeftBracket, RightBracket,
    Comma, Semicolon,
    Plus, Minus, Asterisk, Slash, Percent, Pipe, Ampersand, Caret,
    Equals, ExclamationPoint,
    SingleQuote,

    // Multi-character operators
    // Conditions
    EqualEqual, NotEqual,
    Less, LessEqual,
    Greater, GreaterEqual,
    // Assignment shorthands
    PlusEqual, MinusEqual, AsteriskEqual,
    SlashEqual, PipeEqual, AmpersandEqual, CaretEqual,

    // Literals
    Identifier,
    Number,
    BinaryNumber,
    HexNumber,
    True,
    False,
    Char,
    String,

    // Keywords
    Import,
    If, Else,
    Return,
    While,
    For,
}
