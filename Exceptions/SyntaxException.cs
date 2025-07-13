using System.Text;
using Dirc.Lexing;

class SyntaxException : Exception
{
    private readonly string _message;
    private readonly Token _token;
    private readonly CompilerContext _compilerContext;

    public SyntaxException(string message, Token token, CompilerContext compilerContext) : base(message)
    {
        _message = message;
        _token = token;
        _compilerContext = compilerContext;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        builder.AppendLine($"SyntaxException: {_message} at '{_token.Lexeme}' at {_compilerContext.CurrentFilePath}:line {_token.Line}");
        return builder.ToString();
    }
}
