namespace DircCompiler.Tests;

public class Misc
{
    [Fact]
    public void StraySemicolons()
    {
        string source =
        """
        import print;
        ;
        ;
        print(5);
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
        mov|i1 5 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
