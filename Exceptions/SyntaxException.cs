using System.Text;
using Dirc;
using Dirc.Lexing;

class SyntaxException : Exception
{
    private readonly string _message;
    private readonly Token _token;
    private readonly CompilerContext _compilerContext;
    private readonly CompilerOptions _compilerOptions;

    public SyntaxException(string message, Token token, CompilerOptions compilerOptions, CompilerContext compilerContext) : base(message)
    {
        _message = message;
        _token = token;
        _compilerContext = compilerContext;
        _compilerOptions = compilerOptions;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"SyntaxException: {_message} at '{_token.Lexeme}' at {_compilerContext.CurrentFilePath}:line {_token.Line}");

        if (_compilerOptions.DebugStackTrace)
        {
            builder.AppendLine();
            builder.AppendLine("Debugging stack trace:");
            builder.Append(StackTrace);
        }

        return builder.ToString();
    }
}
