namespace Dirc.Compiling.Tests;

public class IfStatements
{
    [Fact]
    public void TrueStatement()
    {
        string source =
        """
        if (1 == 1) {
            outInt(2);
        }
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifNotEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call @outInt _ _
        label _if0
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void FalseStatement()
    {
        string source =
        """
        if (1 == 2) {
            outInt(2);
        }
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifNotEq|i1|i2 1 2 _if0
        mov|i1 2 _ r0
        call @outInt _ _
        label _if0
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void VariablesInCondition()
    {
        string source =
        """
        int x = 5;
        int y = 4;
        y++;

        if (x == y) {
            outInt(y++);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 5 r0 _
        sub|i2 sp 1 sp
        sub|i2 fp 1 r0
        store|i1 4 r0 _
        sub|i2 fp 1 r0
        load r0 _ r1
        add|i2 r1 1 r0
        sub|i2 fp 1 r1
        store r0 r1 _
        mov fp _ r0
        load r0 _ r1
        sub|i2 fp 1 r0
        load r0 _ r2
        ifNotEq r1 r2 _if0
        sub|i2 fp 1 r0
        load r0 _ r1
        add|i2 r1 1 r0
        sub|i2 fp 1 r1
        store r0 r1 _
        call @outInt _ _
        label _if0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void IfElseTrue()
    {
        string source =
        """
        if (1 == 1) {
            outInt(2);
        }
        else
        {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifNotEq|i1|i2 1 1 _else0
        mov|i1 2 _ r0
        call @outInt _ _
        jump _ifElseEnd0 _ pc
        label _else0
        mov|i1 3 _ r0
        call @outInt _ _
        label _ifElseEnd0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void IfElseFalse()
    {
        string source =
        """
        if (1 == 2) {
            outInt(2);
        }
        else
        {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifNotEq|i1|i2 1 2 _else0
        mov|i1 2 _ r0
        call @outInt _ _
        jump _ifElseEnd0 _ pc
        label _else0
        mov|i1 3 _ r0
        call @outInt _ _
        label _ifElseEnd0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void NotEqual()
    {
        string source =
        """
        if (1 != 1) {
            outInt(2);
        }
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call @outInt _ _
        label _if0
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void Less()
    {
        string source =
        """
        if (1 < 1) {
            outInt(2);
        }
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifMoreOrEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call @outInt _ _
        label _if0
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void LessOrEqual()
    {
        string source =
        """
        if (1 <= 1) {
            outInt(2);
        }
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifMore|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call @outInt _ _
        label _if0
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void More()
    {
        string source =
        """
        if (1 > 1) {
            outInt(2);
        }
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifLessOrEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call @outInt _ _
        label _if0
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void MoreOrEqual()
    {
        string source =
        """
        if (1 >= 1) {
            outInt(2);
        }
        outInt(3);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifLess|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call @outInt _ _
        label _if0
        mov|i1 3 _ r0
        call @outInt _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void ConditionAsExpressionTrue()
    {
        string source =
        """
        outBool(2 == 2);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifNotEq|i1|i2 2 2 _condition0
        mov|i1 1 _ r0
        jump _conditionEnd0 _ pc
        label _condition0
        mov|i1 0 _ r0
        label _conditionEnd0
        call @outBool _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void ConditionAsExpressionFalse()
    {
        string source =
        """
        outBool(1 == 2);
        """.TrimIndents();

        string expected =
        $"""
        label _start
        ifNotEq|i1|i2 1 2 _condition0
        mov|i1 1 _ r0
        jump _conditionEnd0 _ pc
        label _condition0
        mov|i1 0 _ r0
        label _conditionEnd0
        call @outBool _ _
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void NumberAsConditionTrue()
    {
        string source =
        """
        if (1) {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 3 _ r0
        call @outInt _ _
        label _if0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void NumberAsConditionFalse()
    {
        string source =
        """
        if (0) {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        jump _if0 _ pc
        mov|i1 3 _ r0
        call @outInt _ _
        label _if0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void ExpressionAsConditionTrue()
    {
        string source =
        """
        int x = 1;
        if (x) {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 1 r0 _
        mov fp _ r0
        load r0 _ r1
        ifEq|i2 r1 0 _if0
        mov|i1 3 _ r0
        call @outInt _ _
        label _if0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void ExpressionAsConditionFalse()
    {
        string source =
        """
        int x = 0;
        if (x) {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0 r0 _
        mov fp _ r0
        load r0 _ r1
        ifEq|i2 r1 0 _if0
        mov|i1 3 _ r0
        call @outInt _ _
        label _if0
        """.TrimIndents();

        // string assembly = new Compiler().RunFrontEnd(source, new([]), new("unittests", new([source]))).Code;

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BooleanAsConditionTrue()
    {
        string source =
        """
        if (true) {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        mov|i1 3 _ r0
        call @outInt _ _
        label _if0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }

    [Fact]
    public void BooleanAsConditionFalse()
    {
        string source =
        """
        if (false) {
            outInt(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        label _start
        jump _if0 _ pc
        mov|i1 3 _ r0
        call @outInt _ _
        label _if0
        """.TrimIndents();

        Compiler compiler = new Compiler();
        FrontEndResult frontEndResult = compiler.RunFrontEnd(source, new([]), new("unittests", new([source]), true));
        string assembly = compiler.RunBackEnd(frontEndResult.AstNodes, frontEndResult.SymbolTable, new([]), new("unittests", new([source]), true)).Code;

        Assert.Equal(expected, assembly);
    }
}
