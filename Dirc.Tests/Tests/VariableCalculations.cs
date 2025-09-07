namespace Dirc.Compiling.Tests;

public class VariableCalculations
{
    [Fact]
    public void Addition()
    {
        string source =
        """
        int x = 4;
        int y = 3;
        outInt(x + y);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 4 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 3 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        add r1 r2 r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void Subtraction()
    {
        string source =
        """
        int x = 4;
        int y = 3;
        outInt(x - y);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 4 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 3 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        sub r1 r2 r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BitwiseAnd()
    {
        string source =
        """
        int x = 0b11110000;
        int y = 0b00110000;
        outBool(x & y);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b11110000 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 0b00110000 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        and r1 r2 r0
        call @outBool _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BitwiseOr()
    {
        string source =
        """
        int x = 0b11110000;
        int y = 0b00001100;
        outBool(x | y);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b11110000 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 0b00001100 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        or r1 r2 r0
        call @outBool _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BitwiseXor()
    {
        string source =
        """
        int x = 0b11110000;
        int y = 0b00110000;
        outBool(x ^ y);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0b11110000 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 0b00110000 r0 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        xor r1 r2 r0
        call @outBool _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests/test.dirc", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests/test.dirc", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }
}
