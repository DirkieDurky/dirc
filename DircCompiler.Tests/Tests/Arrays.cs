namespace DircCompiler.Tests;

public class Arrays
{
    [Fact]
    public void Declaration()
    {
        string source = "int nums[5];";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void Assignment()
    {
        string source = "int nums[5]; nums[2] = 42;";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void Access()
    {
        string source = "int nums[5]; int x = nums[2];";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void Literal()
    {
        string source = "int nums[5] = {1, 2, 3, 4, 5};";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void WithPrint()
    {
        string source =
        """
        import print;
        
        int nums[5] = {1, 2, 3, 4, 5};
        nums[2] = 42;
        print(nums[2]);
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);

        string assemblyText = string.Join("\n", assembly);
        Assert.Contains("store", assemblyText);
        Assert.Contains("load", assemblyText);
    }
}
