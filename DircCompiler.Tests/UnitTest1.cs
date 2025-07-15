using DircCompiler;

namespace DircCompiler.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        string source =
"""
function myprint(num) {
    print(num + 4);
}

myprint(14);
""";
        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        string expected =
"""
sub|i2 sp 1 sp
jump start _ pc

label print
mov _ r0 out
return _ _ _

label myprint
push lr _ _
push fp _ _
mov sp _ fp
push r0 _ _
add|i2 r0 4 r1
mov r1 _ r0
call print _ _
pop _ _ r0
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _

label start
mov sp _ fp
call myprint _ _
""";

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
