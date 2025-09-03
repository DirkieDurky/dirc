namespace Dirc;

public class BuildOptions
{
    public bool CompileOnly { get; set; } = false; // If true: compile only; if false: also run linker
    public bool EmitMeta { get; set; } = false; // If true: emit a .meta file used for libraries
    public string OutPath { get; set; } = "a.out"; // If true: emit a .meta file used for libraries

    public bool ShowGeneralDebug { get; set; } = false;
    public bool ShowLexerOutput { get; set; } = false;
    public bool ShowParserOutput { get; set; } = false;
    public bool LogAllocation { get; set; } = false;
    public bool DebugStackTrace { get; set; } = false;

    public BuildOptions(List<string> flags)
    {
        foreach (string flag in flags)
        {
            string[] splitString = flag.Split('=');
            if (splitString.Count() > 2)
            {
                Console.WriteLine($"Unknown flag '{flag}'");
                continue;
            }
            switch (splitString[0])
            {
                case "--debug":
                    if (splitString.Count() < 2)
                    {
                        Console.WriteLine("Invalid flag. Please specify which debug options to enable. Options: ['all', 'general', 'lexer', 'parser', 'allocator', 'stack-trace']");
                        break;
                    }
                    SetDebugOptions(splitString[1]);
                    break;
                case "-h":
                case "--help":
                    Console.WriteLine(HelpText);
                    break;
                case "--no-link":
                case "-c":
                    CompileOnly = true;
                    break;
                case "--link":
                    CompileOnly = false;
                    break;
                case "--emit-meta":
                case "-m":
                    EmitMeta = true;
                    break;
                default:
                    Console.WriteLine($"Unknown flag '{flag}'");
                    break;
            }
        }
    }

    public void SetDebugOptions(string debugOptionsStr)
    {
        string[] debugOptions = debugOptionsStr.Split(',');

        foreach (string option in debugOptions)
        {
            switch (option)
            {
                case "all":
                    ShowGeneralDebug = true;
                    ShowLexerOutput = true;
                    ShowParserOutput = true;
                    LogAllocation = true;
                    break;
                case "general":
                    ShowGeneralDebug = true;
                    break;
                case "lexer":
                    ShowLexerOutput = true;
                    break;
                case "parser":
                    ShowParserOutput = true;
                    break;
                case "allocator":
                    LogAllocation = true;
                    break;
                case "stack-trace":
                    DebugStackTrace = true;
                    break;
                default:
                    Console.WriteLine($"Unknown debug option '{option}'");
                    break;
            }
        }
    }

    private const string HelpText = """
usage: dirc <sourcePath> [flags]
flags:
    {-h --help}    View this help.
    {--debug}      Set the amount of debug logging.
    Debug options: ['all', 'general', 'lexer', 'parser', 'allocator', 'stack-trace']
""";
}
