namespace Dirc.Compiling.Tests;

public class Booleans
{
    [Fact]
    public void Test1()
    {
        string source =
        """
        outBool(true);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 1 _ r0
        call @outBool _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }
}
