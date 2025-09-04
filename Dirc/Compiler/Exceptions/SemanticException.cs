using System.Text;
using Dirc;
using Dirc.Compiling;
using Dirc.Compiling.Lexing;

public class SemanticException : Exception
{
    private readonly string _message;
    private readonly Token? _identifierToken;
    private readonly Options _options;
    private readonly BuildContext _buildContext;

    public SemanticException(string message, Token? identifierToken, Options options, BuildContext buildContext) : base(message)
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
        builder.AppendLine($"SemanticException: {_message} at {tokenString} at {_buildContext.CurrentFilePath}{lineString}");

        if (_options.CheckDebugOption(DebugOption.StackTrace))
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
