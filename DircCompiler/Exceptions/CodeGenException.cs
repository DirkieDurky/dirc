using System.Text;
using DircCompiler;
using DircCompiler.Lexing;

public class CodeGenException : Exception
{
    private readonly string _message;
    private readonly Token? _identifierToken;
    private readonly CompilerContext _compilerContext;
    private readonly CompilerOptions _compilerOptions;

    public CodeGenException(string message, Token? identifierToken, CompilerOptions compilerOptions, CompilerContext compilerContext) : base(message)
    {
        _message = message;
        _identifierToken = identifierToken;
        _compilerOptions = compilerOptions;
        _compilerContext = compilerContext;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        string tokenString = _identifierToken != null ? $"'{_identifierToken.Lexeme}'" : "unknown location";
        string lineString = _identifierToken != null ? $":line {_identifierToken.Line}" : "";
        builder.AppendLine($"CodeGenException: {_message} at '{tokenString}' at {_compilerContext.CurrentFilePath}{lineString}");

        if (_compilerOptions.DebugStackTrace)
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
