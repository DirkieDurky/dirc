using Dirc.Compiling.CodeGen;
using Dirc.Compiling.Lexing;

namespace Dirc.Compiling.Parsing;

class Parser
{
    private readonly ParserBase _parserBase;
    private readonly ParserContext _context;

    public Parser(BuildOptions buildOptions, BuildContext buildContext)
    {
        _parserBase = new(buildOptions, buildContext);
        _context = new(_parserBase);
    }

    public FrontEndResult Parse(List<Token> tokens)
    {
        _context.ParserBase.Initialize(tokens);
        List<AstNode> nodes = new();

        while (!_context.ParserBase.IsAtEnd())
        {
            nodes.AddRange(_context.StatementParser.ParseStatement());
        }

        SymbolTable symbolTable = GetSymbolTable(nodes);
        return new FrontEndResult(nodes, symbolTable);
    }

    public SymbolTable GetSymbolTable(List<AstNode> nodes)
    {
        List<MetaFile.Function> functionTable = [];
        foreach (AstNode node in nodes)
        {
            if (node is FunctionDeclarationNode funcNode)
            {
                List<MetaFile.Param> parameters = [];
                foreach (FunctionParameterNode paramNode in funcNode.Parameters)
                {
                    parameters.Add(new()
                    {
                        Name = paramNode.Name,
                        Type = paramNode.Type.Name,
                    });
                }

                functionTable.Add(new()
                {
                    File = Path.GetFileName(_parserBase.Context.CurrentFilePath),
                    Name = funcNode.Name,
                    ReturnType = funcNode.ReturnTypeName,
                    Parameters = parameters.ToArray()
                });
            }
        }

        SymbolTable symbolTable = new SymbolTable();
        symbolTable.FunctionTable = functionTable;

        return symbolTable;
    }
}
