namespace DircCompiler.Tests;

public class Complex
{
    [Fact]
    public void MultipleDefinitions()
    {
        string source =
        """
        void out4(int num) {
            out(num + 4);
        }

        void out25(int num) {
            out(num + 25);
        }

        void out60(int num) {
            out(60 + num);
        }

        void out10(int bum) {
            out(bum + 10);
        }

        out4(14);
        out25(14);
        out60(14);
        out10(14);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerEnvironment.MaxRamValue} _ sp
        mov|i1 {CompilerEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label out4
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        add|i2 r0 4 r1
        mov r1 _ r0
        call out _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label out25
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        add|i2 r0 25 r1
        mov r1 _ r0
        call out _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label out60
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        add|i1 60 r0 r1
        mov r1 _ r0
        call out _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label out10
        push lr _ _
        push fp _ _
        mov sp _ fp
        push r0 _ _
        add|i2 r0 10 r1
        mov r1 _ r0
        call out _ _
        pop _ _ r0
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 14 _ r0
        call out4 _ _
        push r0 _ _
        mov|i1 14 _ r0
        call out25 _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 14 _ r0
        call out60 _ _
        pop _ _ r0
        push r0 _ _
        mov|i1 14 _ r0
        call out10 _ _
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
        import out;
        void test() {
            int x = 5;
            int y = 10;
            int result = x + y;
            out(result);
        } 

        test();
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerEnvironment.MaxRamValue} _ sp
        mov|i1 {CompilerEnvironment.ScreenBufferStart} _ r6
        jump _start _ pc

        label out
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
        call out _ _
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
