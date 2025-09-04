using System.Text;
using Dirc;
using Dirc.Compiling;
using Dirc.Compiling.Lexing;

public class SyntaxException : Exception
{
    private readonly string _message;
    private readonly Token _token;
    private readonly BuildContext _buildContext;
    private readonly Options _options;

    public SyntaxException(string message, Token token, Options options, BuildContext buildContext) : base(message)
    {
        _message = message;
        _token = token;
        _buildContext = buildContext;
        _options = options;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"SyntaxException: {_message} at '{_token.Lexeme}' at {_buildContext.CurrentFilePath}:line {_token.Line}");

        if (_options.CheckDebugOption(DebugOption.StackTrace))
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
