namespace Dirc.Compiling.Tests;

public class SpecialLiterals
{
    [Fact]
    public void BinaryLiterals()
    {
        string source =
        """
        outBool(0b01000000 | 0b00000010);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 0b01000000 | 0b00000010 _ r0
        call @outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void HexLiterals()
    {
        string source =
        """
        outBool(0x0d | 0xd0);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 0x0d | 0xd0 _ r0
        call @outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void AssignBinaryToVariable()
    {
        string source =
        """
        int x = 0b00110011;
        outInt(x);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b00110011 r0 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call @outInt _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void AssignHexToVariable()
    {
        string source =
        """
        int x = 0x0000d0;
        outInt(x);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0x0000d0 r0 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call @outInt _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests", new([source]))).Code;

        Assert.Equal(expected, assembly);
    }
}
