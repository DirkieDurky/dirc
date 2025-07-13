using System.Text;
using Dirc.Lexing;

class CodeGenException : Exception
{
    private readonly string _message;
    private readonly Token _token;
    private readonly CompilerContext _compilerContext;

    public CodeGenException(string message, Token token, CompilerContext compilerContext) : base(message)
    {
        _message = message;
        _token = token;
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
        return builder.ToString();
    }
}
