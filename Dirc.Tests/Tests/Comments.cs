namespace Dirc.Compiling.Tests;

public class Comments
{
    [Fact]
    public void SingleLineComment()
    {
        string source =
        """
        outInt(1);
        // outInt(2);
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 1 _ r0
        call @outInt _ _
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void MultiLineComment()
    {
        string source =
        """
        outInt(1);
        /* outInt(2);
        outInt(3); */
        outInt(4);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 1 _ r0
        call @outInt _ _
        mov|i1 4 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }
}
