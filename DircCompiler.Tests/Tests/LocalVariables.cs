namespace DircCompiler.Tests;

public class LocalVariables
{
    [Fact]
    public void WithoutKeyword()
    {
        string source =
"""
x = 3;
y = 4;
print(x + y);
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
sub|i2 sp 1 sp
mov fp _ r0
store|i1 3 r0 _
sub|i2 sp 1 sp
sub|i2 fp 1 r0
store|i1 4 r0 _
mov fp _ r0
load r0 _ r1
sub|i2 fp 1 r0
load r0 _ r2
add r1 r2 r0
call print _ _
""";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void WithKeyword()
    {
        string source =
"""
var x = 3;
var y = 4;
print(x + y);
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
sub|i2 sp 1 sp
mov fp _ r0
store|i1 3 r0 _
sub|i2 sp 1 sp
sub|i2 fp 1 r0
store|i1 4 r0 _
mov fp _ r0
load r0 _ r1
sub|i2 fp 1 r0
load r0 _ r2
add r1 r2 r0
call print _ _
""";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void ReassignVariable()
    {
        string source =
"""
x = 3;
x = x + 1;

print(x);
""";

        string expected =
"""
?
""";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
