using System.Text.RegularExpressions;

class Parser
{
    public Compiler Compiler;

    public Parser(Compiler compiler)
    {
        Compiler = compiler;
    }

    public int ParseLine(int parsingIndex)
    {
        Regex r;
        Match m;

        r = new Regex("(?:^|;)");
        m = r.Match(Compiler.Input, parsingIndex);
        if (!m.Success) throw new FormatException("Unexpected identifier. Are you missing a ';'?");

        r = new Regex("\\w+\\d*|\\d+");
        m = r.Match(Compiler.Input, parsingIndex);
        if (!m.Success) throw new FormatException("Syntax error");
        string firstWord = m.Value;

        r = new Regex("(?:^|;)function");
        m = r.Match(Compiler.Input, parsingIndex);
        if (m.Success)
        {
            r = new Regex("\\}");
            m = r.Match(Compiler.Input, parsingIndex);
            parsingIndex = m.Index + m.Length;
        }

        // Calling functions
        Function? function = Compiler.Functions.Find(f => f.Name == firstWord);
        if (function != null)
        {
            for (int i = function.Arguments.Count() - 1; i >= 0; i--)
            {
                Compiler.NewLines.Add($"mov _ r{i} stack");
            }

            r = new Regex("\\s*?\\(\\s*(?:(\\w+)\\s*,\\s*)*(\\w+)?\\s*\\)");
            m = r.Match(Compiler.Input, parsingIndex);
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
                string opcode = arguments[i].Immediate ? "mov|imm" : "mov";
                if (arguments[i].Value != $"r{i}")
                {
                    Compiler.NewLines.Add($"{opcode} _ {arguments[i].Value} r{i}");
                }
            }

            Compiler.NewLines.Add($"call _ {function.Name} _");
            for (int i = 0; i < function.Arguments.Count(); i++)
            {
                Compiler.NewLines.Add($"mov _ stack r{i}");
            }

            r = new Regex(";");
            m = r.Match(Compiler.Input, parsingIndex);
            if (!m.Success) throw new FormatException("Missing ';'.");
            parsingIndex = m.Index + m.Length;
        }

        return parsingIndex;
    }

    public Identifier ParseIdentifier(string input)
    {
        Variable? foundArgument = Compiler.KnownArguments.Find(a => a.Name == input);
        if (foundArgument != null)
        {
            return new Identifier(false, foundArgument.Value);
        }
        Variable? foundVariable = Compiler.KnownVariables.Find(a => a.Name == input);
        if (foundVariable != null)
        {
            return new Identifier(false, foundVariable.Value);
        }
        return new Identifier(true, input);
    }
}
