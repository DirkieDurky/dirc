using System.Text;
using Dirc;
using Dirc.Compiling;
using Dirc.Compiling.Lexing;

public class SemanticException : Exception
{
    private readonly string _message;
    private readonly Token? _identifierToken;
    private readonly BuildContext _buildContext;
    private readonly BuildOptions _buildOptions;

    public SemanticException(string message, Token? identifierToken, BuildOptions buildOptions, BuildContext buildContext) : base(message)
    {
        _message = message;
        _identifierToken = identifierToken;
        _buildOptions = buildOptions;
        _buildContext = buildContext;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        string tokenString = _identifierToken != null ? $"'{_identifierToken.Lexeme}'" : "unknown location";
        string lineString = _identifierToken != null ? $":line {_identifierToken.Line}" : "";
        builder.AppendLine($"SemanticException: {_message} at {tokenString} at {_buildContext.CurrentFilePath}{lineString}");

        if (_buildOptions.DebugStackTrace)
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
