namespace DircCompiler.Tests;

public class LocalVariables
{
    [Fact]
    public void WithoutKeyword()
    {
        string source =
        """
        import print;
        
        int x = 3;
        print(x);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 3 r0 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void ReassignVariable()
    {
        string source =
        """
        import print;
        
        int x = 3;
        x = x + 1;

        print(x);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 3 r0 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 1 r0
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
    public void DuplicateDefinition()
    {
        string source =
        """
        import print;
        
        int x = 3;
        int x = 5;

        print(x);
        """.TrimIndents();

        Assert.Throws<SemanticException>(() => new Compiler().Compile(source, new([]), new("unittests")));
    }
}
