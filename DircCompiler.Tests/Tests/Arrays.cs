namespace DircCompiler.Tests;

public class Arrays
{
    [Fact]
    public void TestArrayDeclaration()
    {
        string source = "int nums[5];";

        // This test should compile without errors
        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        // For now, just verify it compiles successfully
        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void TestArrayAssignment()
    {
        string source = "int nums[5]; nums[2] = 42;";

        // This test should compile without errors
        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        // For now, just verify it compiles successfully
        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void TestArrayAccess()
    {
        string source = "int nums[5]; int x = nums[2];";

        // This test should compile without errors
        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        // For now, just verify it compiles successfully
        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void TestArrayLiteral()
    {
        string source = "int nums[5] = {1, 2, 3, 4, 5};";

        // This test should compile without errors
        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        // For now, just verify it compiles successfully
        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);
    }

    [Fact]
    public void TestArrayWithPrint()
    {
        string source =
        """
        import print;
        
        int nums[5] = {1, 2, 3, 4, 5};
        nums[2] = 42;
        print(nums[2]);
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        // Verify it compiles successfully
        Assert.NotNull(assembly);
        Assert.True(assembly.Length > 0);

        // Check that the assembly contains array-related operations
        string assemblyText = string.Join("\n", assembly);
        Assert.Contains("store", assemblyText); // Should contain store operations for array assignment
        Assert.Contains("load", assemblyText);  // Should contain load operations for array access
    }
}
