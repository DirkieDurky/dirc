namespace DircCompiler.Tests;

public class Misc
{
    [Fact]
    public void StraySemicolons()
    {
        string source =
        """
        import out;
        ;
        ;
        out(5);
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
        mov|i1 5 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
