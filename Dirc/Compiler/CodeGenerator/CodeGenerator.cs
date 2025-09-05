using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class CodeGenerator
{
    public CodeGenContext Context { get; init; }
    public LabelGenerator LabelGenerator { get; init; } = new();

    private readonly CodeGenBase _codeGenBase;

    public CodeGenerator(Options options, BuildContext buildContext)
    {
        _codeGenBase = new CodeGenBase();

        Allocator allocator = new(options);

        FunctionFactory funcFactory = new(_codeGenBase, options);
        ExpressionFactory exprFactory = new(_codeGenBase, LabelGenerator);
        VariableFactory varFactory = new(_codeGenBase);
        BinaryFactory binaryFactory = new(_codeGenBase);
        IdentifierFactory identifierFactory = new(_codeGenBase);
        CallFactory callFactory = new(_codeGenBase);
        ControlFlowFactory conditionFactory = new(_codeGenBase);
        ArrayFactory arrayFactory = new(_codeGenBase);
        StringFactory stringFactory = new(_codeGenBase);
        PointerFactory pointerFactory = new(_codeGenBase);
        Context = new CodeGenContext(
            this,
            _codeGenBase,
            allocator,
            exprFactory,
            funcFactory,
            varFactory,
            binaryFactory,
            identifierFactory,
            callFactory,
            conditionFactory,
            arrayFactory,
            stringFactory,
            pointerFactory,
            [],
            [],
            [],
            0,
            options,
            buildContext
        );
    }

    public CompilerResult Generate(List<AstNode> nodes)
    {
        List<string> imports = new();

        foreach (ImportStatementNode importNode in nodes.Where(node => node is ImportStatementNode))
        {
            imports.Add(importNode.LibraryName);
        }

        // Compile functions before rest of the code so they're at the top
        foreach (FunctionDeclarationNode funcNode in nodes.Where(node => node is FunctionDeclarationNode))
        {
            Context.FunctionFactory.Generate(funcNode, (CodeGenContext)Context.CloneAndResetAllocator());
            Context.DeclaredFunctions.Add(funcNode.Name);
        }

        AstNode? topLevelCode = nodes.FirstOrDefault(node => node is not ImportStatementNode && node is not FunctionDeclarationNode);
        if (topLevelCode != null)
        {
            if (!Context.BuildContext.IsRootFile)
            {
                throw new CodeGenException("Non root-file contains top-level code. Only the root file may contain top-level code.", null, Context.Options, Context.BuildContext);
            }
            if (Context.Options.ExportingAsLibrary)
            {
                throw new CodeGenException("File contains top-level code. Libraries may not contain any top-level code.", null, Context.Options, Context.BuildContext);
            }
            _codeGenBase.EmitLabel("_start");
        }

        foreach (AstNode node in nodes)
        {
            switch (node)
            {
                case ImportStatementNode:
                case FunctionDeclarationNode:
                    break;
                default:
                    IReturnable? result = Context.ExprFactory.Generate(node, Context);
                    result?.Free();
                    break;
            }
        }

        _codeGenBase.Code.Length--; // Remove double newline at the end
        return new(_codeGenBase.Code.ToString(), imports.ToArray());
    }
}
