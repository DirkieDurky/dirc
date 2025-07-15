using System.Text;
using DircCompiler;

public class LexicalException : Exception
{
    private readonly string _message;
    private readonly char _character;
    private readonly int _line;
    private readonly CompilerContext _compilerContext;
    private readonly CompilerOptions _compilerOptions;

    public LexicalException(string message, char character, int line, CompilerOptions compilerOptions, CompilerContext compilerContext) : base(message)
    {
        _message = message;
        _character = character;
        _line = line;
        _compilerContext = compilerContext;
        _compilerOptions = compilerOptions;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"LexicalException: {_message} at '{_character}' at {_compilerContext.CurrentFilePath}:line {_line}");

        if (_compilerOptions.DebugStackTrace)
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
