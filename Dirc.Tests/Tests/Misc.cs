namespace Dirc.Compiling.Tests;

public class Misc
{
    [Fact]
    public void StraySemicolons()
    {
        string source =
        """
        ;
        ;
        out(5);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 5 _ r0
        call @out _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }
}
