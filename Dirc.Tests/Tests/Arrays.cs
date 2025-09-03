namespace Dirc.Compiling.Tests;

public class Arrays
{
    [Fact]
    public void Declaration()
    {
        string source = "int nums[5];";

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void Assignment()
    {
        string source = "int nums[5]; nums[2] = 42;";

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void Access()
    {
        string source = "int nums[5]; int x = nums[2];";

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void Literal()
    {
        string source = "int nums[5] = {1, 2, 3, 4, 5};";

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void WithPrint()
    {
        string source =
        """
        int nums[5] = {1, 2, 3, 4, 5};
        nums[2] = 42;
        outInt(nums[2]);
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);

        string assemblyText = string.Join("\n", assembly);
        Assert.Contains("store", assemblyText);
        Assert.Contains("load", assemblyText);
    }
}
