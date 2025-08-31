namespace Dirc.Compiling.Tests;

public class Calculations
{
    [Fact]
    public void Addition()
    {
        string source =
        """
        out(4 + 3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {BuildEnvironment.MaxRamValue} _ sp
        mov|i1 {BuildEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 4 + 3 _ r0
        call out _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void Subtraction()
    {
        string source =
        """
        out(4 - 3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {BuildEnvironment.MaxRamValue} _ sp
        mov|i1 {BuildEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 4 - 3 _ r0
        call out _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

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
        mov|i1 {BuildEnvironment.MaxRamValue} _ sp
        mov|i1 {BuildEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0b11110000 & 0b00110000 _ r0
        call outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

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
        mov|i1 {BuildEnvironment.MaxRamValue} _ sp
        mov|i1 {BuildEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0b11110000 | 0b00001100 _ r0
        call outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

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
        mov|i1 {BuildEnvironment.MaxRamValue} _ sp
        mov|i1 {BuildEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0b11110000 ^ 0b00110000 _ r0
        call outBool _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }
}
