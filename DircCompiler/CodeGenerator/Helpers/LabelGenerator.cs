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

    public string Generate(LabelType prefix) => $"_{prefix.ToString().ToLower()}{_counter[prefix]++}";
}

enum LabelType
{
    If,
}
