namespace Dirc.Compiling.Tests;

public class Functions
{
    [Fact]
    public void StandardFunctionCall()
    {
        string source =
        """
        outInt(14);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 14 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void CustomFunction()
    {
        string source =
        """
        void myoutInt(int num) {
            outInt(num);
        }
        """.TrimIndents();

        string expected =
        $"""
        label myoutInt
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call @outInt _ _
        mov r0 _ r1
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _
        
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void NestedCall()
    {
        string source =
        """
        void myoutInt(int num) {
            outInt(num);
        }

        myoutInt(14);
        """.TrimIndents();

        string expected =
        $"""
        label myoutInt
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call @outInt _ _
        mov r0 _ r1
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov|i1 14 _ r0
        call myoutInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void MultipleCalls()
    {
        string source =
        """
        void myoutInt(int num) {
            outInt(num);
        }

        myoutInt(14);
        myoutInt(8);
        myoutInt(25);
        myoutInt(128);
        myoutInt(255);
        """.TrimIndents();

        string expected =
        $"""
        label myoutInt
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        call @outInt _ _
        mov r0 _ r1
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov|i1 14 _ r0
        call myoutInt _ _
        mov|i1 8 _ r0
        call myoutInt _ _
        mov|i1 25 _ r0
        call myoutInt _ _
        mov|i1 128 _ r0
        call myoutInt _ _
        mov|i1 255 _ r0
        call myoutInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void DefineExistingStandardFunction()
    {
        string source =
        """
        void outInt() {
            outInt(3);
        }
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        Assert.Throws<SemanticException>(() => compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)));
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

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        Assert.Throws<SemanticException>(() => compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)));
    }

    [Fact]
    public void CallUnknownFunction()
    {
        string source =
        """
        asd();
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        Assert.Throws<SemanticException>(() => compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)));
    }

    [Fact]
    public void CallsOutOfOrder()
    {
        string source =
        """
        test();

        void rest() {
            outInt(1);
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
        call @outInt _ _
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

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;
        Assert.Equal(expected, assembly);
    }
}
