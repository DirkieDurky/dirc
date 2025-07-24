namespace DircCompiler.Tests;

public class Pointer
{
    [Fact]
    public void AddressOfAndDereference()
    {
        string source =
        """
        int x = 5;
        int* p = &x;
        int y = *p;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 5 r0 _
        sub|i2 sp 1 sp
        mov fp _ r0
        sub|i2 fp 1 r1
        store r0 r1 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        load r0 _ r1
        load r1 _ r0
        sub|i2 fp 2 r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void PointerFunctionParameterAndReturn()
    {
        string source =
        """
        int getValue(int* p) { return *p; }
        int x = 7;
        int* p = &x;
        int y = getValue(p);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label getValue
        push lr _ _
        push fp _ _
        mov sp _ fp
        load r0 _ r1
        mov r1 _ r0
        return _ _ _
        mov fp _ sp
        pop _ _ fp
        pop _ _ lr
        return _ _ _

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 7 r0 _
        sub|i2 sp 1 sp
        mov fp _ r0
        sub|i2 fp 1 r1
        store r0 r1 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        load r0 _ r1
        mov r1 _ r0
        call getValue _ _
        sub|i2 fp 2 r1
        store r0 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void PointerDereferenceStoresValue()
    {
        string source =
        """
        int x = 42;
        int* p = &x;
        *p = 99;
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 42 r0 _
        sub|i2 sp 1 sp
        mov fp _ r0
        sub|i2 fp 1 r1
        store r0 r1 _
        sub|i2 fp 1 r0
        load r0 _ r1
        store|i1 99 r1 _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void Simple()
    {
        string source =
        """
        import print;

        int* a = 5;
        *a = 10;
        print(*a);
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
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 5 r0 _
        mov fp _ r0
        load r0 _ r1
        store|i1 10 r1 _
        mov fp _ r0
        load r0 _ r1
        load r1 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
