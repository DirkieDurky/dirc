using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dirc.Compiling.CodeGen;

namespace Dirc.Linking;

partial class Linker
{
    public string Link(string assembly, List<string> otherCompilationUnitCode, string[] searchLibs)
    {
        HashSet<string> toImport = GetLibsToImport(assembly, searchLibs.ToList(), []);

        StringBuilder result = new();
        result.Append(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "lib", "runtime", "init.o")));
        result.AppendLine();
        foreach (string importPath in toImport)
        {
            result.Append(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "lib", importPath)));
            result.AppendLine();
        }
        foreach (string code in otherCompilationUnitCode)
        {
            result.Append(code);
            result.AppendLine();
        }
        result.Append(assembly);

        result = result.Replace("[SCREEN_PTR]", BuildEnvironment.ScreenPointerAddress.ToString());

        string resultStr = result.ToString();
        resultStr = UnresolvedSymbol().Replace(resultStr, "$1");

        return resultStr;
    }

    public HashSet<string> GetLibsToImport(string assembly, List<string> searchLibs, HashSet<string> libsToImport)
    {
        if (!searchLibs.Contains("stdlib")) searchLibs.Insert(0, "stdlib");

        List<string> unresolvedSymbols = UnresolvedSymbol().Matches(assembly).Select(x => x.Groups[1].ToString()).ToList();

        foreach (string symbol in unresolvedSymbols)
        {
            // Look in the runtime library
            if (RuntimeLibrary.HasFunction(symbol))
            {
                libsToImport = GetLibsToImport(RuntimeLibrary.GetFunction(symbol), searchLibs, libsToImport);
                libsToImport.Add($"runtime/{symbol}.o");
            }
            else
            {
                foreach (string searchPath in searchLibs)
                {
                    //Finally look at search paths
                    string libPath = Path.Combine(AppContext.BaseDirectory, "lib", searchPath, $"{searchPath}.o");
                    string libMetaPath = Path.Combine(AppContext.BaseDirectory, "lib", searchPath, $"{searchPath}.meta");
                    string libText = File.ReadAllText(libPath);
                    string libMetaText = File.ReadAllText(libMetaPath);
                    MetaFile.Root libMetaFile = JsonSerializer.Deserialize<MetaFile.Root>(libMetaText)
                        ?? throw new Exception($"Standard Library meta file could not be read");

                    if (libMetaFile.Functions.Any(f => f.Name == symbol))
                    {
                        libsToImport = GetLibsToImport(libText, searchLibs, libsToImport);
                        libsToImport.Add(libPath);
                    }
                }
            }
        }

        return libsToImport;
    }

    [GeneratedRegex("@([a-zA-Z]\\w*)")]
    private static partial Regex UnresolvedSymbol();
}
