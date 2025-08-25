namespace DircCompiler.Tests;

public class Booleans
{
    [Fact]
    public void Test1()
    {
        string source =
        """
        import outBool;
        outBool(true);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 1 _ r0
        call outBool _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
