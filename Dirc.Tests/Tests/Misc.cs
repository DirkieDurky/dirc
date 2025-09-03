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
        outInt(5);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 5 _ r0
        call @outInt _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }
}
