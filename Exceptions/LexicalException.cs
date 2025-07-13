using System.Text;

class LexicalException : Exception
{
    private readonly string _message;
    private readonly char _character;
    private readonly int _line;
    private readonly CompilerContext _compilerContext;

    public LexicalException(string message, char character, int line, CompilerContext compilerContext) : base(message)
    {
        _message = message;
        _character = character;
        _line = line;
        _compilerContext = compilerContext;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"LexicalException: {_message} at '{_character}' at {_compilerContext.CurrentFilePath}:line {_line}");
        return builder.ToString();
    }
}
