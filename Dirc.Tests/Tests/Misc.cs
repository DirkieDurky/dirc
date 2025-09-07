namespace Dirc.Compiling.Tests;

public class Misc
{
    [Fact]
    public void StraySemicolons()
    {
        string source =
        """
        ;
        ;
        outInt(5);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 5 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }
}
