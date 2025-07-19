namespace DircCompiler.Tests;

public class Functions
{
    [Fact]
    public void StandardFunctionCall()
    {
        string source =
        """
        import print;

        print(14);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 14 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void CustomFunction()
    {
        string source =
        """
        import print;

        void myprint(int num) {
            print(num);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label myprint
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call print _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov sp _ fp
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void NestedCall()
    {
        string source =
        """
        import print;

        void myprint(int num) {
            print(num);
        }

        myprint(14);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label myprint
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call print _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 14 _ r0
        call myprint _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void MultipleCalls()
    {
        string source =
        """
        import print;

        void myprint(int num) {
            print(num);
        }

        myprint(14);
        myprint(8);
        myprint(25);
        myprint(128);
        myprint(255);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label myprint
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call print _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 14 _ r0
        call myprint _ _
        push r0 _ _
        mov|i1 8 _ r0
        call myprint _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 25 _ r0
        call myprint _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 128 _ r0
        call myprint _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 255 _ r0
        call myprint _ _
        pop _ _ r0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void DefineExistingStandardFunction()
    {
        string source =
        """
        import print;

        void print() {
            print(3);
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
        import print;

        test();

        void rest() {
            print(1);
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
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label rest
        push lr _ _
        push fp _ _
        mov sp _ fp
        mov|i1 1 _ r0
        call print _ _
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
        mov sp _ fp
        call test _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));
        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
