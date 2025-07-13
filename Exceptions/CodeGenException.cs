using System.Text;
using Dirc;
using Dirc.Lexing;

class CodeGenException : Exception
{
    private readonly string _message;
    private readonly Token _token;
    private readonly CompilerContext _compilerContext;
    private readonly CompilerOptions _compilerOptions;

    public CodeGenException(string message, Token token, CompilerOptions compilerOptions, CompilerContext compilerContext) : base(message)
    {
        _message = message;
        _token = token;
        _compilerOptions = compilerOptions;
        _compilerContext = compilerContext;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"CodeGenException: {_message} at '{_token.Lexeme}'");
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
