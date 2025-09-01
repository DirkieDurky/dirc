using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;

namespace Dirc.Compiling.CodeGen;

class CodeGenerator
{
    public CodeGenContext Context { get; init; }
    public LabelGenerator LabelGenerator { get; init; } = new();

    private readonly CodeGenBase _codeGenBase;

    public CodeGenerator(BuildOptions buildOptions, BuildContext buildContext)
    {
        _codeGenBase = new CodeGenBase();

        Allocator allocator = new(buildOptions);

        FunctionFactory funcFactory = new FunctionFactory(_codeGenBase, buildOptions);
        ExpressionFactory exprFactory = new ExpressionFactory(_codeGenBase, LabelGenerator);
        VariableFactory varFactory = new VariableFactory(_codeGenBase);
        BinaryFactory binaryFactory = new BinaryFactory(_codeGenBase);
        IdentifierFactory identifierFactory = new IdentifierFactory(_codeGenBase);
        CallFactory callFactory = new CallFactory(_codeGenBase);
        ControlFlowFactory conditionFactory = new ControlFlowFactory(_codeGenBase);
        ArrayFactory arrayFactory = new ArrayFactory(_codeGenBase);
        PointerFactory pointerFactory = new PointerFactory(_codeGenBase);
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
            pointerFactory,
            [],
            [],
            [],
            0,
            buildOptions,
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

        if (nodes.Any(node => node is not ImportStatementNode && node is not FunctionDeclarationNode))
            _codeGenBase.EmitLabel("_start");

        foreach (AstNode node in nodes)
        {
            switch (node)
            {
                case ImportStatementNode:
                case FunctionDeclarationNode:
                    break;
                default:
                    Context.ExprFactory.Generate(node, Context);
                    break;
            }
        }

        _codeGenBase.Code.Length--; // Remove double newline at the end
        return new(_codeGenBase.Code.ToString(), imports.ToArray());
    }
}
