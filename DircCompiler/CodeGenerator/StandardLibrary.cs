using DircCompiler.Lexing;
using DircCompiler.Parsing;
using DircCompiler.Semantic;

namespace DircCompiler.CodeGen;

static class StandardLibrary
{
    private static Token T(string lexeme) => new Token(TokenType.Identifier, lexeme, null, -1);

    private static readonly Dictionary<string, StandardFunction> _functions = new()
    {
        {"out", new StandardFunction("out",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("int"), "int"), "value")]
        ),
            "mov r0 _ out"
        )},
        {"outBool", new StandardFunction("outBool",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("bool"), "bool"), "value")]
        ),
            "mov r0 _ out"
        )},
        {"outChar", new StandardFunction("outChar",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("char"), "char"), "value")]
        ),
            "mov r0 _ out"
        )},
        {"printChar", new StandardFunction("printChar",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("value"), new NamedTypeNode(T("char"), "char"), "value")]
        ),
            """
            store r0 {{SCREEN_PTR}} _
            add|i2 {{SCREEN_PTR}} 1 {{SCREEN_PTR}}
            """.TrimIndents()
        )},
        {"input", new StandardFunction("input",
        new FunctionSignature(
            Int.Instance,
            []
        ),
            "mov in _ r0"
        )},
        { "malloc", new StandardFunction("malloc",
        new FunctionSignature(
            Pointer.Of(Semantic.Void.Instance),
            [new FunctionParameterNode(T("size"), new NamedTypeNode(T("int"), "int"), "size")]
        ),
            // r0 = size
            // r1 = currentAddress
            // r2 = currentValue
            """
            mov|i1 0 _ r1
            label findFreeLoop
            load r1 _ r2
            ifNotEq|i2 r2 0 notAvailable
            add|i2 r1 1 r1
            load r1 _ r2
            ifEq|i2 r2 0 allocate
            load r1 _ r2
            ifMore r2 r0 allocate
            label notAvailable
            add|i2 r1 1 r1
            load r1 _ r2
            add r1 r2 r1
            add|i2 r1 1 r1
            jump findFreeLoop _ pc

            label allocate
            sub|i2 r1 1 r1
            store|i1 1 r1 _
            add|i2 r1 1 r1
            load r1 _ r2
            ifNotEq|i2 r2 0 dontOverride
            store r0 r1 _
            label dontOverride
            add|i2 r1 1 r0
            """.TrimIndents()
        )},
        { "free", new StandardFunction("free",
        new FunctionSignature(
            Semantic.Void.Instance,
            [new FunctionParameterNode(T("ptr"), new PointerTypeNode(T("void*"), new NamedTypeNode(T("void"), "void")), "ptr")]
        ),
            """
            sub|i2 r0 2 r0
            store|i1 0 r0 _
            """.TrimIndents()
        )}
    };

    public static bool HasFunction(string name)
    {
        return _functions.ContainsKey(name);
    }

    public static StandardFunction GetFunction(string name, CodeGenContext? context)
    {
        StandardFunction function = _functions[name];
        if (context != null)
        {
            function.Code = function.Code.Replace("{{SCREEN_PTR}}", context.ScreenPtr.RegisterEnum.ToString());
        }
        return function;
    }
}
