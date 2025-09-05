using CommandLine;

namespace Dirc;

public class Options
{
    [Option('c', "no-link", Required = false, Default = false, HelpText = "Don't run linker after compiling.")]
    public bool CompileOnly { get; set; } = false;

    private string? _libName;
    [Option('m', "make-lib", MetaValue = "libname", Required = false, HelpText = "Compile files as library.")]
    public string? LibName
    {
        get => _libName;
        set
        {
            _libName = value;
            ExportingAsLibrary = value != null;
        }
    }
    public bool ExportingAsLibrary { get; set; } = false;

    [Option('o', "out-file", MetaValue = "filename", Required = false, Default = "'a.out' when executable", HelpText = "Output file path when compiling to an executable. Output folder when compiling to a library.")]
    public string? OutPath { get; set; } = null;

    // If true, ignores functions from stdlib when checking for duplicate functions. Useful for compiling stdlib itself
    [Option("ignore-stdlib", Required = false, Hidden = true)]
    public bool IgnoreStdlib { get; set; } = false;

    private List<DebugOption> _debugOptions { get; } = [];
    [Option("debug", MetaValue = "option...", Required = false, Separator = ',', HelpText = "Set debug mode. Valid values: General, Lexer, Parser, Allocator, StackTrace, None, All")]
    public IEnumerable<string> StringDebugOptions
    {
        set
        {
            foreach (string str in value)
            {
                if (!Enum.TryParse(str, true, out DebugOption result))
                {
                    Console.WriteLine($"Warning: invalid debug option given: {str}");
                }
                else
                {
                    _debugOptions.Add(result);
                }
            }
        }
    }

    [Value(0, MetaName = "input files", HelpText = "The input files to compile.")]
    public IEnumerable<string> InputFiles { get; set; }

    public bool CheckDebugOption(DebugOption debugOption)
    {
        return _debugOptions.Contains(DebugOption.All) || _debugOptions.Contains(debugOption);
    }

    public Options(IEnumerable<string> inputFiles)
    {
        InputFiles = inputFiles;
    }

    public Options()
    {
        InputFiles = [];
    }
}

[Flags]
public enum DebugOption
{
    General,
    Lexer,
    Parser,
    Allocator,
    StackTrace,
    None,
    All,
}
