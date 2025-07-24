namespace DircCompiler.Tests;

public class Pointers
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

    [Fact]
    public void PointerArithmetic()
    {
        string source =
        """
        import print;

        int* a = 5;
        *(a) = 10;
        *(a + 1) = 12;
        *(a + 2) = 9;
        *(a + 3) = 4;
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
        add|i2 r1 1 r0
        store|i1 12 r0 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 2 r0
        store|i1 9 r0 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 3 r0
        store|i1 4 r0 _
        mov fp _ r0
        load r0 _ r1
        load r1 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void HeapArray()
    {
        string source =
        """
        import print;

        int* a = 5;
        a[0] = 10;
        a[1] = 12;
        a[2] = 9;
        a[3] = 4;
        print(a[2]);
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
        mov r1 _ r2
        store|i1 10 r2 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 1 r2
        store|i1 12 r2 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 2 r2
        store|i1 9 r2 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 3 r2
        store|i1 4 r2 _
        mov fp _ r0
        load r0 _ r1
        add|i2 r1 2 r2
        load r2 _ r0
        call print _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
