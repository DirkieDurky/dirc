namespace Dirc.Compiling;

public enum Comparer
{
    IfEq,
    IfNotEq,
    IfLess,
    IfLessOrEq,
    IfMore,
    IfMoreOrEq,
}

public static class ComparerExtensions
{
    public static Comparer GetOpposite(this Comparer comparer)
    {
        return comparer switch
        {
            Comparer.IfEq => Comparer.IfNotEq,
            Comparer.IfNotEq => Comparer.IfEq,
            Comparer.IfLess => Comparer.IfMoreOrEq,
            Comparer.IfMoreOrEq => Comparer.IfLess,
            Comparer.IfMore => Comparer.IfLessOrEq,
            Comparer.IfLessOrEq => Comparer.IfMore,
            _ => throw new Exception("Unknown comparer")
        };
    }

    public static Operation ToOperation(this Comparer comparer)
    {
        return comparer switch
        {
            Comparer.IfEq => Operation.Equal,
            Comparer.IfNotEq => Operation.NotEqual,
            Comparer.IfLess => Operation.LessThan,
            Comparer.IfMoreOrEq => Operation.GreaterEqual,
            Comparer.IfMore => Operation.GreaterThan,
            Comparer.IfLessOrEq => Operation.LessEqual,
            _ => throw new Exception("Unknown comparer")
        };
    }
}
