using DircCompiler.Lexing;
using DircCompiler.Parsing;
using DircCompiler.Semantic;

namespace DircCompiler.CodeGen;

static class StandardLibrary
{
    private static Token T(string lexeme) => new Token(TokenType.Identifier, lexeme, null, -1);

    public static Dictionary<string, StandardFunction> Functions = new()
    {
        {"print", new StandardFunction("print",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("int"), "int"), "value")]
        ),
            ["mov r0 _ out"]
        )},
        {"printBool", new StandardFunction("printBool",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("bool"), "bool"), "value")]
        ),
            ["mov r0 _ out"]
        )},
        {"input", new StandardFunction("input",
        new FunctionSignature(
            Int.Instance,
            []
        ),
            ["mov in _ r0"]
        )},
    };
}
