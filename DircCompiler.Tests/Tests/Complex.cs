namespace DircCompiler.Tests;

public class Complex
{
    [Fact]
    public void MultipleDefinitions()
    {
        string source =
        """
        import print;
        void print4(int num) {
            print(num + 4);
        }

        void print25(int num) {
            print(num + 25);
        }

        void print60(int num) {
            print(60 + num);
        }

        void print10(int bum) {
            print(bum + 10);
        }

        print4(14);
        print25(14);
        print60(14);
        print10(14);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label print4
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

        label print25
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        add|i2 r0 25 r1
        mov r1 _ r0
        call print _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label print60
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        add|i1 60 r0 r1
        mov r1 _ r0
        call print _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label print10
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        add|i2 r0 10 r1
        mov r1 _ r0
        call print _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 14 _ r0
        call print4 _ _
        push r0 _ _
        mov|i1 14 _ r0
        call print25 _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 14 _ r0
        call print60 _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 14 _ r0
        call print10 _ _
        pop _ _ r0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void VariablesAndFunctions()
    {
        string source =
        """
        import print;
        void test() {
            int x = 5;
            int y = 10;
            int result = x + y;
            print(result);
        } 

        test();
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label print
        mov r0 _ out
        return _ _ _

        label test
        push lr _ _
        push fp _ _
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 5 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 10 r0 _
        sub|i2 sp 1 sp
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        add r1 r2 r0
        sub|i2 fp 2 r1
        store r0 r1 _
        sub|i2 fp 2 r0
        load r0 _ r1
        mov r1 _ r0
        call print _ _
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
