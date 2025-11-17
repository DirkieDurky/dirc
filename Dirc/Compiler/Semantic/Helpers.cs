namespace Dirc.Compiling.Semantic;

class Helpers
{
    public static Dictionary<Operation, SimpleType> ReturnTypes = new() {
        {Operation.Add, Int.Instance},
        {Operation.And, Bool.Instance},
        {Operation.Div, Int.Instance},
        {Operation.Equal, Bool.Instance},
        {Operation.GreaterEqual, Bool.Instance},
        {Operation.GreaterThan, Bool.Instance},
        {Operation.LessEqual, Bool.Instance},
        {Operation.LessThan, Bool.Instance},
        {Operation.Mod, Int.Instance},
        {Operation.Mul, Int.Instance},
        {Operation.Not, Bool.Instance},
        {Operation.NotEqual, Bool.Instance},
        {Operation.Or, Bool.Instance},
        {Operation.Sub, Int.Instance},
        {Operation.Xor, Bool.Instance},
    };
}
