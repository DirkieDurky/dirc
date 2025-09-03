namespace Dirc.Compiling.Tests;

public class Calculations
{
    [Fact]
    public void Addition()
    {
        string source =
        """
        outInt(4 + 3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 4 + 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        string assembly = new Compiler().RunFrontEnd(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void Subtraction()
    {
        string source =
        """
        outInt(4 - 3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 4 - 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        string assembly = new Compiler().RunFrontEnd(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BitwiseAnd()
    {
        string source =
        """
        outBool(0b11110000 & 0b00110000);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 0b11110000 & 0b00110000 _ r0
        call @outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().RunFrontEnd(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BitwiseOr()
    {
        string source =
        """
        outBool(0b11110000 | 0b00001100);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 0b11110000 | 0b00001100 _ r0
        call @outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().RunFrontEnd(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BitwiseXor()
    {
        string source =
        """
        outBool(0b11110000 ^ 0b00110000);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 0b11110000 ^ 0b00110000 _ r0
        call @outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().RunFrontEnd(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }
}
