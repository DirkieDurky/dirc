namespace Dirc.Compiling.Tests;

public class Booleans
{
    [Fact]
    public void Test1()
    {
        string source =
        """
        outBool(true);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 1 _ r0
        call @outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }
}
