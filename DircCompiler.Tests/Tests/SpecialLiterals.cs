namespace DircCompiler.Tests;

public class SpecialLiterals
{
    [Fact]
    public void BinaryLiterals()
    {
        string source =
"""
print(0b01000000 | 0b00000010);
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
mov|i1 0b01000000 | 0b00000010 _ r0
call print _ _
""";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void HexLiterals()
    {
        string source =
"""
print(0x0d | 0xd0);
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
mov|i1 0x0d | 0xd0 _ r0
call print _ _
""";

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
