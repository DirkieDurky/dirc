namespace DircCompiler.Tests;

public class Calculations
{
    [Fact]
    public void Addition()
    {
        string source =
"""
print(4 + 3);
""";

        string expected =
"""
sub|i2 sp 1 sp
jump start _ pc

label print
mov _ r0 out
return _ _ _

label start
mov sp _ fp
mov|i1 4 + 3 _ r0
call print _ _
""";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
