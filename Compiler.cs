using System.Text.RegularExpressions;

class Compiler
{
    public string Input;
    public List<string> NewLines = [];
    public List<Function> Functions = [];
    public List<Variable> KnownArguments = [];
    public List<Variable> KnownVariables = [];

    private Parser Parser;

    public Compiler(string input)
    {
        Input = input.Replace("\n", " ").Replace("\r", " ").Trim();
        Parser = new(this);
    }

    public string[] Compile(bool root = true)
    {
        int parsingIndex = 0;

        if (root)
        {
            InitializeCompilation(Input);
        }

        while (parsingIndex < Input.Length)
        {
            parsingIndex = Parser.ParseLine(parsingIndex);
        }

        return NewLines.ToArray();
    }

    public void InitializeCompilation(string input)
    {
        int parsingIndex;
        Regex r;
        Match m;

        NewLines.Add("jump _ start cnt");
        NewLines.Add("");

        StandardLibrary std = new(this);
        std.Compile();

        // Compile function definitions
        r = new Regex("(?:^|;)function");
        MatchCollection matches = r.Matches(input);

        foreach (Match match in matches)
        {
            parsingIndex = match.Index + match.Length;

            r = new Regex("\\s+(\\w+)");
            m = r.Match(input, parsingIndex);
            if (!m.Success) throw new FormatException("Functions should get a function name");
            parsingIndex = m.Index + m.Length;
            string functionName = m.Groups[1].Value;

            r = new Regex("\\s*?\\(\\s*(?:(\\w+)\\s*,\\s*)*(\\w+)?\\s*\\)");
            m = r.Match(input, parsingIndex);
            if (!m.Success) throw new FormatException("Functions should be followed by parentheses");
            parsingIndex = m.Index + m.Length;

            List<string> arguments = [];
            if (!string.IsNullOrEmpty(m.Groups[1].Value))
            {
                arguments.AddRange(m.Groups[1].Value.Replace(" ", "").Split().ToList());
            }
            arguments.Add(m.Groups[2].Value);

            r = new Regex("\\s*?\\{(.*?)\\}");
            m = r.Match(input, parsingIndex);
            if (!m.Success) throw new FormatException("Missing code block after function declaration");
            parsingIndex = m.Index + m.Length;
            string code = m.Groups[1].Value;

            CompileFunction(functionName, arguments.ToArray(), code);
        }

        NewLines.Add("label start");
    }

    // Takes D code and compiles it
    public void CompileFunction(string name, string[] arguments, string code)
    {
        int startingLine = (NewLines.Count + 1) * 4;

        List<Variable> functionArguments = [];
        for (int i = 0; i < arguments.Length; i++)
        {
            functionArguments.Add(new Variable(arguments[i], $"r{i}"));
        }

        Compiler compiler = new Compiler(code);
        compiler.Functions = Functions;
        compiler.KnownArguments = functionArguments;

        NewLines.Add($"label {name}");
        NewLines.AddRange(compiler.Compile(false));
        NewLines.Add($"return _ _ _");
        NewLines.Add("");

        Functions.Add(new Function(name, arguments, startingLine, true));
    }

    // Takes DIRIC Assembly and simply adds that to the code
    public void CompileStandardFunction(string functionName, string[] arguments, string[] code)
    {
        int startingLine = (NewLines.Count + 1) * 4;

        NewLines.Add($"label {functionName}");
        NewLines.AddRange(code);
        NewLines.Add("return _ _ _");
        NewLines.Add("");

        Functions.Add(new Function(functionName, arguments, startingLine, false));
    }
}
