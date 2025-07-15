namespace DircCompiler;

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
            _ => throw new Exception("Invalid operation given")
        };
    }
}
