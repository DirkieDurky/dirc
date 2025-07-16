namespace DircCompiler.CodeGen;

class LabelGenerator
{
    private Dictionary<LabelType, int> _counter = new();

    public LabelGenerator()
    {
        foreach (LabelType labelType in Enum.GetValues(typeof(LabelType)))
        {
            _counter.Add(labelType, 0);
        }
    }

    public string Generate(LabelType prefix) => $"_{FirstCharToLowerCase(prefix.ToString())}{_counter[prefix]++}";

    public static string? FirstCharToLowerCase(string? str)
    {
        if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];

        return str;
    }
}

enum LabelType
{
    If, Else, IfElseEnd, Condition, ConditionEnd,
}
