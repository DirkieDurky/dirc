using System.Text.RegularExpressions;

class Compiler
{
    public string Input;
    public List<string> NewLines = [];
    public List<Function> Functions = [];
    public List<Variable> KnownArguments = [];
    public List<Variable> KnownVariables = [];

    public Compiler(string input)
    {
        Input = input.Replace("\n", " ").Replace("\r", " ").Trim();
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
            parsingIndex = ParseLine(parsingIndex);
        }

        return NewLines.ToArray();
    }

    public int ParseLine(int parsingIndex)
    {
        Regex r;
        Match m;

        r = new Regex("(?:^|;)");
        m = r.Match(Input, parsingIndex);
        if (!m.Success) throw new FormatException("Unexpected identifier. Are you missing a ';'?");

        r = new Regex("\\w+\\d*|\\d+");
        m = r.Match(Input, parsingIndex);
        if (!m.Success) throw new FormatException("Syntax error");
        string firstWord = m.Value;

        r = new Regex("(?:^|;)function");
        m = r.Match(Input, parsingIndex);
        if (m.Success)
        {
            r = new Regex("\\}");
            m = r.Match(Input, parsingIndex);
            parsingIndex = m.Index + m.Length;
        }
        // Calling functions
        Function? function = Functions.Find(f => f.Name == firstWord);
        if (function != null)
        {
            for (int i = function.Arguments.Count() - 1; i >= 0; i--)
            {
                NewLines.Add($"mov _ r{i} stack");
            }

            r = new Regex("\\s*?\\(\\s*(?:(\\w+)\\s*,\\s*)*(\\w+)?\\s*\\)");
            m = r.Match(Input, parsingIndex);
            if (!m.Success) throw new FormatException("Functions should be followed by parentheses");
            parsingIndex = m.Index + m.Length;

            List<string> argumentsAsStr = [];
            if (!string.IsNullOrEmpty(m.Groups[1].Value))
            {
                argumentsAsStr.AddRange(m.Groups[1].Value.Replace(" ", "").Split().ToList());
            }
            argumentsAsStr.Add(m.Groups[2].Value);

            List<Identifier> arguments = argumentsAsStr.Select(ParseIdentifier).ToList();

            if (arguments.Count() != function.Arguments.Count())
                throw new FormatException($"Function '{function.Name}' takes {function.Arguments.Count()} arguments. {arguments.Count()} given.");

            for (int i = 0; i < arguments.Count(); i++)
            {
                string opcode = arguments[i].Immediate ? "movI" : "mov";
                NewLines.Add($"{opcode} _ {arguments[i].Value} r{i}");
            }

            NewLines.Add($"call _ {function.Name} _");
            for (int i = 0; i < function.Arguments.Count(); i++)
            {
                NewLines.Add($"mov _ stack r{i}");
            }

            r = new Regex(";");
            m = r.Match(Input, parsingIndex);
            if (!m.Success) throw new FormatException("Missing ';'.");
            parsingIndex = m.Index + m.Length;
        }

        return parsingIndex;
    }

    public Identifier ParseIdentifier(string input)
    {
        Variable? foundArgument = KnownArguments.Find(a => a.Name == input);
        if (foundArgument != null)
        {
            return new Identifier(false, foundArgument.Value);
        }
        Variable? foundVariable = KnownVariables.Find(a => a.Name == input);
        if (foundVariable != null)
        {
            return new Identifier(false, foundVariable.Value);
        }
        return new Identifier(true, input);
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
