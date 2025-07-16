namespace DircCompiler.Tests;

public class ForStatements
{
    [Fact]
    public void Simple()
    {
        string source =
        """
        import print;

        for (i = 0; i < 5; i++) {
            print(i);
        }
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
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0 r0 _
        label _while0
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call print _ _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 1 r0
        mov fp _ r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        ifLess|i2 r1 5 _while0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
