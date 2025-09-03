namespace Dirc.Compiling.Tests;

public class LocalVariables
{
    [Fact]
    public void WithoutKeyword()
    {
        string source =
        """
        int x = 3;
        outInt(x);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 3 r0 _
        mov fp _ r0
        load r0 _ r1
        mov r1 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void ReassignVariable()
    {
        string source =
        """
        int x = 3;
        x = x + 1;

        outInt(x);
        """.TrimIndents();

        string expected =
        $"""
        label _start
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
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void DuplicateDefinition()
    {
        string source =
        """
        int x = 3;
        int x = 5;

        outInt(x);
        """.TrimIndents();

        Assert.Throws<SemanticException>(() => new Compiler().RunFrontEnd(source, new([]), new("unittests", new([source]), true)));
    }
}
