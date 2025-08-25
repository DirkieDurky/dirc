namespace DircCompiler.Tests;

public class IfStatements
{
    [Fact]
    public void TrueStatement()
    {
        string source =
        """
        import out;

        if (1 == 1) {
            out(2);
        }
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifNotEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call out _ _
        label _if0
        mov|i1 3 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void FalseStatement()
    {
        string source =
        """
        import out;
        
        if (1 == 2) {
            out(2);
        }
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifNotEq|i1|i2 1 2 _if0
        mov|i1 2 _ r0
        call out _ _
        label _if0
        mov|i1 3 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void VariablesInCondition()
    {
        string source =
        """
        import out;
        
        int x = 5;
        int y = 4;
        y++;

        if (x == y) {
            out(y++);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
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
        call out _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void IfElseTrue()
    {
        string source =
        """
        import out;
        
        if (1 == 1) {
            out(2);
        }
        else
        {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifNotEq|i1|i2 1 1 _else0
        mov|i1 2 _ r0
        call out _ _
        jump _ifElseEnd0 _ pc
        label _else0
        mov|i1 3 _ r0
        call out _ _
        label _ifElseEnd0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void IfElseFalse()
    {
        string source =
        """
        import out;
        
        if (1 == 2) {
            out(2);
        }
        else
        {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifNotEq|i1|i2 1 2 _else0
        mov|i1 2 _ r0
        call out _ _
        jump _ifElseEnd0 _ pc
        label _else0
        mov|i1 3 _ r0
        call out _ _
        label _ifElseEnd0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void NotEqual()
    {
        string source =
        """
        import out;
        
        if (1 != 1) {
            out(2);
        }
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call out _ _
        label _if0
        mov|i1 3 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void Less()
    {
        string source =
        """
        import out;
        
        if (1 < 1) {
            out(2);
        }
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifMoreOrEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call out _ _
        label _if0
        mov|i1 3 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void LessOrEqual()
    {
        string source =
        """
        import out;
        
        if (1 <= 1) {
            out(2);
        }
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifMore|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call out _ _
        label _if0
        mov|i1 3 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void More()
    {
        string source =
        """
        import out;
        
        if (1 > 1) {
            out(2);
        }
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifLessOrEq|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call out _ _
        label _if0
        mov|i1 3 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void MoreOrEqual()
    {
        string source =
        """
        import out;
        
        if (1 >= 1) {
            out(2);
        }
        out(3);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifLess|i1|i2 1 1 _if0
        mov|i1 2 _ r0
        call out _ _
        label _if0
        mov|i1 3 _ r0
        call out _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void ConditionAsExpressionTrue()
    {
        string source =
        """
        import outBool;
        
        outBool(2 == 2);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifNotEq|i1|i2 2 2 _condition0
        mov|i1 1 _ r0
        jump _conditionEnd0 _ pc
        label _condition0
        mov|i1 0 _ r0
        label _conditionEnd0
        call outBool _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void ConditionAsExpressionFalse()
    {
        string source =
        """
        import outBool;

        outBool(1 == 2);
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label outBool
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        ifNotEq|i1|i2 1 2 _condition0
        mov|i1 1 _ r0
        jump _conditionEnd0 _ pc
        label _condition0
        mov|i1 0 _ r0
        label _conditionEnd0
        call outBool _ _
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void NumberAsConditionTrue()
    {
        string source =
        """
        import out;

        if (1) {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 3 _ r0
        call out _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void NumberAsConditionFalse()
    {
        string source =
        """
        import out;

        if (0) {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        jump _if0 _ pc
        mov|i1 3 _ r0
        call out _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void ExpressionAsConditionTrue()
    {
        string source =
        """
        import out;

        int x = 1;
        if (x) {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 1 r0 _
        mov fp _ r0
        load r0 _ r1
        ifEq|i2 r1 0 _if0
        mov|i1 3 _ r0
        call out _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void ExpressionAsConditionFalse()
    {
        string source =
        """
        import out;

        int x = 0;
        if (x) {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        sub|i2 sp 1 sp
        mov fp _ r0
        store|i1 0 r0 _
        mov fp _ r0
        load r0 _ r1
        ifEq|i2 r1 0 _if0
        mov|i1 3 _ r0
        call out _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BooleanAsConditionTrue()
    {
        string source =
        """
        import out;

        if (true) {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        mov|i1 3 _ r0
        call out _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }

    [Fact]
    public void BooleanAsConditionFalse()
    {
        string source =
        """
        import out;

        if (false) {
            out(3);
        }
        """.TrimIndents();

        string expected =
        $"""
        mov|i1 {CompilerContext.MaxRamValue} _ sp
        jump _start _ pc

        label out
        mov r0 _ out
        return _ _ _

        label _start
        mov sp _ fp
        jump _if0 _ pc
        mov|i1 3 _ r0
        call out _ _
        label _if0
        """.TrimIndents();

        string[] assembly = new Compiler().Compile(source, new([]), new("unittests"));

        Assert.Equal(expected.Split(Environment.NewLine), assembly);
    }
}
