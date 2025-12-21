using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Parsing;
using Dirc.HAL;

namespace Dirc.Compiling.CodeGen;

class CodeGenerator
{
    public CodeGenContext Context { get; init; }
    public LabelGenerator LabelGenerator { get; init; } = new();

    private readonly ICodeGenBase _codeGenBase;
    private readonly BuildEnvironment _buildEnvironment;

    public CodeGenerator(Options options, BuildContext buildContext, BuildEnvironment buildEnvironment)
    {
        _buildEnvironment = buildEnvironment;
        _codeGenBase = options.TargetArchitecture.CodeGenBase;

        Allocator allocator = new(options, buildContext);

        FunctionFactory funcFactory = new(_codeGenBase, options);
        ExpressionFactory exprFactory = new(_codeGenBase, LabelGenerator);
        VariableFactory varFactory = new(_codeGenBase);
        BinaryFactory binaryFactory = new(_codeGenBase);
        UnaryFactory unaryFactory = new(_codeGenBase);
        IdentifierFactory identifierFactory = new(_codeGenBase);
        CallFactory callFactory = new(_codeGenBase);
        ControlFlowFactory conditionFactory = new(_codeGenBase);
        ArrayFactory arrayFactory = new(_codeGenBase);
        StringFactory stringFactory = new(_codeGenBase);
        PointerFactory pointerFactory = new(_codeGenBase);
        Dictionary<string, Variable> variableTable = new();

        foreach (GlobalConstant globalConstant in BuildEnvironment.GlobalConstants)
        {
            variableTable.Add(globalConstant.Name, new DirectVariable(globalConstant.Name, globalConstant.Value));
        }

        Context = new CodeGenContext(
            this,
            _codeGenBase,
            allocator,
            exprFactory,
            funcFactory,
            varFactory,
            binaryFactory,
            unaryFactory,
            identifierFactory,
            callFactory,
            conditionFactory,
            arrayFactory,
            stringFactory,
            pointerFactory,
            variableTable,
            [],
            0,
            0,
            0,
            options,
            buildContext,
            buildEnvironment,
            new()
        );
    }

    public CompilerResult Generate(List<AstNode> nodes)
    {
        List<string> imports = new();

        foreach (ImportStatementNode importNode in nodes.Where(node => node is ImportStatementNode))
        {
            imports.Add(importNode.LibraryName);
        }

        // Initialize DeclaredFunctions
        foreach (FunctionDeclarationNode funcNode in nodes.Where(node => node is FunctionDeclarationNode))
        {
            Context.DeclaredFunctions.Add(funcNode.Name);
        }

        // Compile functions before rest of the code so they're at the top
        foreach (FunctionDeclarationNode funcNode in nodes.Where(node => node is FunctionDeclarationNode))
        {
            Context.FunctionFactory.Generate(funcNode, (CodeGenContext)Context.GetSubcontext());
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
            _codeGenBase.EmitStartLabel();
            Context.StackframeSize = Context.FunctionFactory.CalculateStackframeSize(nodes);
            Context.FunctionFactory.AllocateStackframe(Context);
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

        if (topLevelCode != null)
        {
            _codeGenBase.EmitHalt();
        }

        if (_codeGenBase.Code.Length > 0) _codeGenBase.Code.Length--; // Remove double newline at the end
        return new(_codeGenBase.Code.ToString(), imports.ToArray());
    }
}
