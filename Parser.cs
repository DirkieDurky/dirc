using System.Net.Http.Headers;
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

            r = new Regex("\\s*?\\(");
            m = r.Match(Compiler.Input, parsingIndex);
            if (!m.Success) throw new FormatException("Functions should be followed by parentheses");
            parsingIndex = m.Index + m.Length;

            Regex closingParanthesisRegex = new Regex("^\\s*?\\)");
            m = closingParanthesisRegex.Match(Compiler.Input.Substring(parsingIndex));
            List<Identifier> arguments = [];
            if (!m.Success)
            {
                do
                {
                    r = new Regex("[\\s\\w\\d+\\-\\*/%|&^]+,?");
                    m = r.Match(Compiler.Input, parsingIndex);
                    if (!m.Success) throw new FormatException("Invalid argument syntax");
                    arguments.Add(ParseIdentifier(m.Groups[0].Value));
                    parsingIndex = m.Index + m.Length;
                } while (m.Value.Last() == ',');
            }
            m = closingParanthesisRegex.Match(Compiler.Input.Substring(parsingIndex));
            if (!m.Success) throw new FormatException("Unexpected closing parentesis");

            if (arguments.Count() != function.Arguments.Count())
                throw new FormatException($"Function '{function.Name}' takes {function.Arguments.Count()} arguments. {arguments.Count()} given.");

            for (int i = 0; i < arguments.Count(); i++)
            {
                string opcode = "mov";
                opcode += arguments[i].Type == Identifier.IdentifierType.Immediate ? "|imm2" : "";

                string value = arguments[i].Value;
                if (arguments[i].Type == Identifier.IdentifierType.RamPointer)
                {
                    Compiler.NewLines.Add($"mov|imm2 _ {arguments[i].Value} ramPointer");
                    value = "ram";
                }
                if (arguments[i].Value != $"r{i}" || arguments[i].Type == Identifier.IdentifierType.RamPointer)
                {
                    Compiler.NewLines.Add($"{opcode} _ {value} r{i}");
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
        Regex r;
        Match m;

        // If only immediate values we can just pass it through because the assembler can deal with that
        r = new Regex("(\\d+)(?:\\s*[+\\-\\*/%|&^]\\s*(\\d+))+");
        m = r.Match(input);
        if (m.Success)
        {
            return new Identifier(Identifier.IdentifierType.Immediate, input);
        }
        r = new Regex("((?:\\w+\\d*)|\\d+)\\s*([+\\-\\*/%|&^])\\s*((?:\\w+\\d*)|\\d+)");
        m = r.Match(input);
        if (m.Success)
        {
            string operation = m.Groups[2].Value switch
            {
                "+" => "add",
                "-" => "sub",
                "|" => "or",
                "&" => "and",
                "^" => "xor",
                _ => throw new InvalidOperationException("No such operation"),
            };

            Identifier operand1 = ParseIdentifier(m.Groups[1].Value);
            Identifier operand2 = ParseIdentifier(m.Groups[3].Value);

            if (operand1.Type == Identifier.IdentifierType.Immediate)
            {
                operation += "|imm1";
            }
            if (operand2.Type == Identifier.IdentifierType.Immediate)
            {
                operation += "|imm2";
            }

            Compiler.NewLines.Add("mov|imm2 _ 0 ramPointer");
            Compiler.NewLines.Add($"{operation} {operand1.Value} {operand2.Value} ram");
            return new Identifier(Identifier.IdentifierType.RamPointer, "0");
        }

        Variable? foundArgument = Compiler.KnownArguments.Find(a => a.Name == input);
        if (foundArgument != null)
        {
            return new Identifier(Identifier.IdentifierType.Register, foundArgument.Value);
        }
        Variable? foundVariable = Compiler.KnownVariables.Find(a => a.Name == input);
        if (foundVariable != null)
        {
            return new Identifier(Identifier.IdentifierType.RamPointer, foundVariable.Value);
        }
        return new Identifier(Identifier.IdentifierType.Immediate, input);
    }
}
