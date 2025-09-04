using System.Text;
using Dirc;
using Dirc.Compiling;
using Dirc.Compiling.Lexing;

public class CodeGenException : Exception
{
    private readonly string _message;
    private readonly Token? _identifierToken;
    private readonly BuildContext _buildContext;
    private readonly Options _options;

    public CodeGenException(string message, Token? identifierToken, Options options, BuildContext buildContext) : base(message)
    {
        _message = message;
        _identifierToken = identifierToken;
        _options = options;
        _buildContext = buildContext;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        string tokenString = _identifierToken != null ? $"'{_identifierToken.Lexeme}'" : "unknown location";
        string lineString = _identifierToken != null ? $":line {_identifierToken.Line}" : "";
        builder.AppendLine($"CodeGenException: {_message} at {tokenString} at {_buildContext.CurrentFilePath}{lineString}");

        if (_options.CheckDebugOption(DebugOption.StackTrace))
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
