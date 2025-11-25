namespace Dirc.Compiling;

public enum Operation
{
    Add,
    Sub,
    And,
    Or,
    Not,
    Xor,
    Mul,
    Div,
    Mod,
    Equal,
    NotEqual,
    LessThan,
    LessEqual,
    GreaterThan,
    GreaterEqual,
    BitshiftLeft,
    BitshiftRight,
}

public static class OperationExtensions
{
    public static string GetInlineString(this Operation operation)
    {
        return operation switch
        {
            Operation.Add => "+",
            Operation.Sub => "-",
            Operation.And => "&",
            Operation.Or => "|",
            Operation.Not => throw new Exception("Operation does not have an inline string representation"),
            Operation.Xor => "^",
            Operation.Mul => "*",
            Operation.Div => "/",
            Operation.Mod => "%",
            Operation.Equal => "==",
            Operation.NotEqual => "!=",
            Operation.LessThan => "<",
            Operation.LessEqual => "<=",
            Operation.GreaterThan => ">",
            Operation.GreaterEqual => ">=",
            _ => throw new Exception("Invalid operation given")
        };
    }

    public static bool IsComparer(this Operation operation)
    {
        return operation == Operation.Equal
        || operation == Operation.NotEqual
        || operation == Operation.LessThan
        || operation == Operation.LessEqual
        || operation == Operation.GreaterThan
        || operation == Operation.GreaterEqual;
    }

    public static Comparer GetComparer(this Operation operation)
    {
        return operation switch
        {
            Operation.Equal => Comparer.IfEq,
            Operation.NotEqual => Comparer.IfNotEq,
            Operation.LessThan => Comparer.IfLess,
            Operation.LessEqual => Comparer.IfLessOrEq,
            Operation.GreaterThan => Comparer.IfMore,
            Operation.GreaterEqual => Comparer.IfMoreOrEq,
            _ => throw new Exception("Operation is not a comparer")
        };
    }
}
