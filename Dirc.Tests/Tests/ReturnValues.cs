namespace Dirc.Compiling.Tests;

public class ReturnValues
{
    [Fact]
    public void StandardFunction()
    {
        string source =
        """
        int x = input();
        out(x);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        call @input _ _
        mov fp _ r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call @out _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void StandardFunctionDirect()
    {
        string source =
        """
        out(input());
        """.TrimIndents();

        string expected =
        $"""
        label _start
        call @input _ _
        call @out _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void CustomFunction()
    {
        string source =
        """
        int myAdd(int x, int y) {
            return x + y;
        }

        out(myAdd(1, 2));
        """.TrimIndents();

        string expected =
        $"""
        label myAdd
        push lr _ _
        push fp _ _
        mov sp _ fp
        add r0 r1 r2
        mov r2 _ r0
        return _ _ _
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov|i1 1 _ r0
        mov|i1 2 _ r1
        call myAdd _ _
        call @out _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }
}
