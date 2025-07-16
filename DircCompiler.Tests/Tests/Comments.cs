namespace DircCompiler.Tests;

public class Comments
{
    [Fact]
    public void SingleLineComment()
    {
        string source =
        """
        import print;

        print(1);
        // print(2);
        print(3);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 1 _ r0
        call print _ _
        push r0 _ _
        mov|i1 3 _ r0
        call print _ _
        pop _ _ r0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void MultiLineComment()
    {
        string source =
        """
        import print;

        print(1);
        /* print(2);
        print(3); */
        print(4);
        """.TrimIndents();

        string expected =
        """
        sub|i2 sp 1 sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 1 _ r0
        call print _ _
        push r0 _ _
        mov|i1 4 _ r0
        call print _ _
        pop _ _ r0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
