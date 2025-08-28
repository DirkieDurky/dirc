namespace DircCompiler.Tests;

public class Comments
{
    [Fact]
    public void SingleLineComment()
    {
        string source =
        """
        import out;

        out(1);
        // out(2);
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerEnvironment.MaxRamValue} _ sp
        mov|i1 {CompilerEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 1 _ r0
        call out _ _
        push r0 _ _
        mov|i1 3 _ r0
        call out _ _
        pop _ _ r0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void MultiLineComment()
    {
        string source =
        """
        import out;

        out(1);
        /* out(2);
        out(3); */
        out(4);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerEnvironment.MaxRamValue} _ sp
        mov|i1 {CompilerEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 1 _ r0
        call out _ _
        push r0 _ _
        mov|i1 4 _ r0
        call out _ _
        pop _ _ r0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
