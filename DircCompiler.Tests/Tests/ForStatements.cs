namespace DircCompiler.Tests;

public class ForStatements
{
    [Fact]
    public void Simple()
    {
        string source =
        """
        import out;

        for (int i = 0; i < 5; i++) {
            out(i);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0 r0 _
        label _while0
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call out _ _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 1 r0
        mov fp _ r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        ifLess|i2 r1 5 _while0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void EmptyExpressions()
    {
        string source =
        """
        import out;

        for (;;) {
            out(4);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        label _while0
        mov|i1 4 _ r0
        call out _ _
        jump _while0 _ pc
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
