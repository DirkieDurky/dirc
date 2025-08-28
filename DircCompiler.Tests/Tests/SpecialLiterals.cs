namespace DircCompiler.Tests;

public class SpecialLiterals
{
    [Fact]
    public void BinaryLiterals()
    {
        string source =
        """
        import outBool;

        outBool(0b01000000 | 0b00000010);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerEnvironment.MaxRamValue} _ sp
        mov|i1 {CompilerEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0b01000000 | 0b00000010 _ r0
        call outBool _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void HexLiterals()
    {
        string source =
        """
        import outBool;

        outBool(0x0d | 0xd0);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerEnvironment.MaxRamValue} _ sp
        mov|i1 {CompilerEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0x0d | 0xd0 _ r0
        call outBool _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void AssignBinaryToVariable()
    {
        string source =
        """
        import out;

        int x = 0b00110011;
        out(x);
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
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b00110011 r0 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void AssignHexToVariable()
    {
        string source =
        """
        import out;

        int x = 0x0000d0;
        out(x);
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
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0x0000d0 r0 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
