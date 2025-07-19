namespace DircCompiler.Tests;

public class ReturnValues
{
    [Fact]
    public void StandardFunction()
    {
        string source =
        """
        import input;
        import print;

        int x = input();
        print(x);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label input
        mov in _ r0
        return _ _ _

        label print
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        call input _ _
        mov fp _ r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void StandardFunctionDirect()
    {
        string source =
        """
        import input;
        import print;

        print(input());
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label input
        mov in _ r0
        return _ _ _

        label print
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        call input _ _
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void CustomFunction()
    {
        string source =
        """
        import print;

        int myAdd(int x, int y) {
            return x + y;
        }

        print(myAdd(1, 2));
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

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
        mov sp _ fp
        mov|i1 1 _ r0
        mov|i1 2 _ r1
        call myAdd _ _
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
