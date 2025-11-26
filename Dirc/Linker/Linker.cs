using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dirc.Compiling.CodeGen;

namespace Dirc.Linking;

partial class Linker
{
    public string Link(string assembly, List<string> otherCompilationUnitCode, string[] searchLibs)
    {
        HashSet<string> toImport = GetFilesToImport(assembly, searchLibs.ToList(), []);

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
        result = result.Replace("[MAX_RAM_ADDRESS]", BuildEnvironment.MaxRamAddress.ToString());
        result = result.Replace("[SCREEN_BUFFER_START]", BuildEnvironment.ScreenBufferStart.ToString());

        string resultStr = result.ToString();
        resultStr = UnresolvedSymbol().Replace(resultStr, "$1");

        return resultStr;
    }

    public HashSet<string> GetFilesToImport(string assembly, List<string> searchLibs, HashSet<string> libsToImport)
    {
        if (!searchLibs.Contains("stdlib")) searchLibs.Insert(0, "stdlib");

        List<string> unresolvedSymbols = UnresolvedSymbol().Matches(assembly).Select(x => x.Groups[1].ToString()).ToList();

        foreach (string symbol in unresolvedSymbols)
        {
            // Look in the runtime library
            if (RuntimeLibrary.HasFunction(symbol))
            {
                libsToImport = GetFilesToImport(RuntimeLibrary.GetFunction(symbol), searchLibs, libsToImport);
                libsToImport.Add($"runtime/{symbol}.o");
            }
            else
            {
                foreach (string searchPath in searchLibs)
                {
                    //Finally look at search paths
                    string libPath = Path.Combine(AppContext.BaseDirectory, "lib", searchPath);
                    string libMetaPath = Path.Combine(libPath, $"{searchPath}.meta");
                    string libMetaText = File.ReadAllText(libMetaPath);
                    MetaFile.Root libMetaFile = JsonSerializer.Deserialize<MetaFile.Root>(libMetaText)
                        ?? throw new Exception($"Library meta file could not be read");

                    MetaFile.Function? function = libMetaFile.Functions.FirstOrDefault(f => f.Name == symbol);
                    if (function != null)
                    {
                        string filePath = Path.Combine(libPath, function.File);
                        string fileText = File.ReadAllText(filePath);
                        if (!libsToImport.Contains(filePath))
                        {
                            libsToImport.Add(filePath);
                            libsToImport = GetFilesToImport(fileText, searchLibs, libsToImport);
                        }
                    }
                }
            }
        }

        return libsToImport;
    }

    [GeneratedRegex("@([a-zA-Z]\\w*)")]
    private static partial Regex UnresolvedSymbol();
}
