namespace Dirc.Compiling.Tests;

public class Functions
{
    [Fact]
    public void StandardFunctionCall()
    {
        string source =
        """
        out(14);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 14 _ r0
        call @out _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void CustomFunction()
    {
        string source =
        """
        void myout(int num) {
            out(num);
        }
        """.TrimIndents();

        string expected =
        $"""
        label myout
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call @out _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _
        
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void NestedCall()
    {
        string source =
        """
        void myout(int num) {
            out(num);
        }

        myout(14);
        """.TrimIndents();

        string expected =
        $"""
        label myout
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call @out _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov|i1 14 _ r0
        call myout _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void MultipleCalls()
    {
        string source =
        """
        void myout(int num) {
            out(num);
        }

        myout(14);
        myout(8);
        myout(25);
        myout(128);
        myout(255);
        """.TrimIndents();

        string expected =
        $"""
        label myout
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call @out _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov|i1 14 _ r0
        call myout _ _
        push r0 _ _
        mov|i1 8 _ r0
        call myout _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 25 _ r0
        call myout _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 128 _ r0
        call myout _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 255 _ r0
        call myout _ _
        pop _ _ r0
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void DefineExistingStandardFunction()
    {
        string source =
        """
        void out() {
            out(3);
        }
        """.TrimIndents();

        Assert.Throws<SemanticException>(() => new Compiler().Compile(source, new([]), new("unittests")));
    }

    [Fact]
    public void DuplicateDefinition()
    {
        string source =
        """
        void asd() {

        }

        void asd() {
            
        }
        """.TrimIndents();

        Assert.Throws<SemanticException>(() => new Compiler().Compile(source, new([]), new("unittests")));
    }

    [Fact]
    public void CallUnknownFunction()
    {
        string source =
        """
        asd();
        """.TrimIndents();

        Assert.Throws<SemanticException>(() => new Compiler().Compile(source, new([]), new("unittests")));
    }

    [Fact]
    public void CallsOutOfOrder()
    {
        string source =
        """
        test();

        void rest() {
            out(1);
        }

        void best() {
            rest();
        }

        void test() {
            best();
        }
        """.TrimIndents();

        string expected =
        $"""
        label rest
        push lr _ _
        push fp _ _
        mov sp _ fp
        mov|i1 1 _ r0
        call @out _ _
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label best
        push lr _ _
        push fp _ _
        mov sp _ fp
        call rest _ _
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label test
        push lr _ _
        push fp _ _
        mov sp _ fp
        call best _ _
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        call test _ _
        """.TrimIndents();

        string assembly = new Compiler().Compile(source, new([]), new("unittests")).Code;
        Assert.Equal(expected, assembly);
    }
}
