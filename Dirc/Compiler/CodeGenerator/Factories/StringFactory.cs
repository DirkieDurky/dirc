using System.Runtime.InteropServices;
using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class StringFactory
{
    private readonly ICodeGenBase _codeGenBase;

    public StringFactory(ICodeGenBase codeGenBase)
    {
        _codeGenBase = codeGenBase;
    }

    public ReturnRegister GenerateStringLiteralReturnBasePtr(StringLiteralNode stringLiteral, CodeGenContext context)
    {
        return new ReturnRegister(GenerateStringLiteral(stringLiteral, context).BasePtr);
    }

    public ArrayLiteral GenerateStringLiteral(StringLiteralNode stringLiteral, CodeGenContext context)
    {
        List<AstNode> chars = new();
        foreach (char c in stringLiteral.Str.Literal!.ToString()!)
        {
            chars.Add(new CharNode(c));
        }
        chars.Add(new CharNode('\0'));
        ArrayLiteralNode arrayLiteralNode = new(chars);
        return context.ArrayFactory.GenerateArrayLiteral(arrayLiteralNode, context);
    }

    public ArrayLiteralNode ToArrayLiteral(StringLiteralNode stringLiteral)
    {
        List<AstNode> chars = new();
        foreach (char c in stringLiteral.Str.Literal!.ToString()!)
        {
            chars.Add(new CharNode(c));
        }
        chars.Add(new CharNode('\0'));
        return new(chars);
    }
}
