using System.Text;
using Dirc;
using Dirc.Compiling;

public class LexicalException : Exception
{
    private readonly string _message;
    private readonly char _character;
    private readonly int _line;
    private readonly BuildContext _buildContext;
    private readonly Options _options;

    public LexicalException(string message, char character, int line, Options options, BuildContext buildContext) : base(message)
    {
        _message = message;
        _character = character;
        _line = line;
        _buildContext = buildContext;
        _options = options;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"LexicalException: {_message} at '{_character}' at {_buildContext.CurrentFilePath}:line {_line}");

        if (_options.CheckDebugOption(DebugOption.StackTrace))
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
