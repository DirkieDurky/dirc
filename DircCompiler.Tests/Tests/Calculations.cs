namespace DircCompiler.Tests;

public class Calculations
{
    [Fact]
    public void Addition()
    {
        string source =
        """
        print(4 + 3);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump _start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 4 + 3 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void Subtraction()
    {
        string source =
        """
        print(4 - 3);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump _start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 4 - 3 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BitwiseAnd()
    {
        string source =
        """
        print(0b11110000 & 0b00110000);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump _start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0b11110000 & 0b00110000 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BitwiseOr()
    {
        string source =
        """
        print(0b11110000 | 0b00001100);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump _start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0b11110000 | 0b00001100 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BitwiseXor()
    {
        string source =
        """
        print(0b11110000 ^ 0b00110000);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump _start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 0b11110000 ^ 0b00110000 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
