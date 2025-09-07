namespace Dirc.Compiling.Tests;

public class ForStatements
{
    [Fact]
    public void Simple()
    {
        string source =
        """
        for (int i = 0; i < 5; i++) {
            outInt(i);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0 r0 _
        label _while0
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call @outInt _ _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 1 r0
        mov fp _ r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        ifLess|i2 r1 5 _while0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void EmptyExpressions()
    {
        string source =
        """
        for (;;) {
            outInt(4);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        label _while0
        mov|i1 4 _ r0
        call @outInt _ _
        jump _while0 _ pc
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }
}
