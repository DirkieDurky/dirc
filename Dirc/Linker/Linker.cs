using System.Formats.Asn1;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Dirc.Compiling.CodeGen;

namespace Dirc.Linking;

partial class Linker
{
    public string Link(string assembly, List<string> otherCompilationUnitCode, string[] searchLibs, Options options)
    {
        HashSet<string> toImport = GetFilesToImport(assembly, searchLibs.ToList(), [], options);

        StringBuilder result = new();
        result.Append(File.ReadAllText(Path.Combine(options.TargetArchitecture.RuntimeLibrary.GetPath(), "init.o")));
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

        foreach (GlobalConstant globalConstant in BuildEnvironment.GlobalConstants)
        {
            result = result.Replace('[' + globalConstant.Name + ']', globalConstant.Value.ToString());
        }

        string resultStr = result.ToString();
        resultStr = UnresolvedSymbol().Replace(resultStr, "$1");

        return resultStr;
    }

    public HashSet<string> GetFilesToImport(string assembly, List<string> searchLibs, HashSet<string> libsToImport, Options options)
    {
        if (!searchLibs.Contains("stdlib")) searchLibs.Insert(0, "stdlib");
        string stdcoreLibraryName = options.TargetArchitecture.StandardCoreLibraryName;
        if (!searchLibs.Contains(stdcoreLibraryName)) searchLibs.Insert(0, stdcoreLibraryName);

        List<string> unresolvedSymbols = UnresolvedSymbol().Matches(assembly).Select(x => x.Groups[1].ToString()).ToList();

        foreach (string symbol in unresolvedSymbols)
        {
            // Look in the runtime library
            if (options.TargetArchitecture.RuntimeLibrary.HasFunction(symbol))
            {
                libsToImport = GetFilesToImport(options.TargetArchitecture.RuntimeLibrary.GetFunction(symbol), searchLibs, libsToImport, options);
                libsToImport.Add($"{options.TargetArchitecture.RuntimeLibrary.GetName()}/{symbol}.o");
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
                            libsToImport = GetFilesToImport(fileText, searchLibs, libsToImport, options);
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
