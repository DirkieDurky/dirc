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
        builder.AppendLine($"CodeGenException: {_message} at {tokenString}");
        foreach ((string filename, int line) in _compilerContext.GetCallStack())
        {
            builder.AppendLine($"at {filename}:line {line}");
        }

        if (_compilerOptions.DebugStackTrace)
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
