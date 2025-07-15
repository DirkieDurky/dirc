namespace DircCompiler.Tests;

public class IfStatements
{
    [Fact]
    public void TrueStatement()
    {
        string source =
        """
        if (1 == 1) {
            print(2);
        }
        print(3);
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
        ifNotEq 1 1 _if0
        mov|i1 2 _ r0
        call print _ _
        label _if0
        mov|i1 3 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void FalseStatement()
    {
        string source =
        """
        if (1 == 2) {
            print(2);
        }
        print(3);
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
        ifNotEq 1 2 _if0
        mov|i1 2 _ r0
        call print _ _
        label _if0
        mov|i1 3 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void VariablesInCondition()
    {
        string source =
        """
        x = 5;
        y = 4;
        y++;

        if (x == y) {
            print(y++);
        }
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
        store|i1 5 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 4 r0 _
        sub|i2 fp 1 r0
        load r0 _ r1
        add|i2 r1 1 r0
        sub|i2 fp 1 r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        ifNotEq r1 r2 _if0
        sub|i2 fp 1 r0
        load r0 _ r1
        add|i2 r1 1 r0
        sub|i2 fp 1 r1
        store r0 r1 _
        call print _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
