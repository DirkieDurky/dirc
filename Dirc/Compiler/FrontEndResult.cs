using Dirc.Compiling.Parsing;

namespace Dirc.Compiling;

public class FrontEndResult(List<AstNode> astNodes, SymbolTable symbolTable)
{
    public List<AstNode> AstNodes = astNodes;
    public SymbolTable SymbolTable = symbolTable;
}
