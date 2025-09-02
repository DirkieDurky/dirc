namespace Dirc.Compiling.Tests;

public class WhileStatements
{
    [Fact]
    public void Simple()
    {
        string source =
        """
        int i = 0;
        while (i < 5) {
            outInt(i);
            i++;
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0 r0 _
        label _while0
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call @outInt _ _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 1 r0
        mov fp _ r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        ifLess|i2 r1 5 _while0
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void WhileTrue()
    {
        string source =
        """
        while (true) {
            outInt(42);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        label _while0
        mov|i1 42 _ r0
        call @outInt _ _
        jump _while0 _ pc
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }
}
