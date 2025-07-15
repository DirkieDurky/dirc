namespace DircCompiler.Tests;

public class VariableCalculations
{
    [Fact]
    public void Addition()
    {
        string source =
        """
        x = 4;
        y = 3;
        print(x + y);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 4 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 3 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        add r1 r2 r0
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
        x = 4;
        y = 3;
        print(x - y);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 4 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 3 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        sub r1 r2 r0
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
        x = 0b11110000;
        y = 0b00110000;
        print(x & y);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b11110000 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 0b00110000 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        and r1 r2 r0
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
        x = 0b11110000;
        y = 0b00001100;
        print(x | y);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b11110000 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 0b00001100 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        or r1 r2 r0
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
        x = 0b11110000;
        y = 0b00110000;
        print(x ^ y);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump start _ pc

        label print
        mov _ r0 out
        return _ _ _

        label start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b11110000 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 0b00110000 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        xor r1 r2 r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
