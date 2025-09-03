using Dirc.Compiling;
using Dirc.Compiling.Parsing;
using Dirc.Linking;

namespace Dirc;

public class Driver
{
    public void Run(List<string> files, BuildOptions buildOptions)
    {
        string rootFile = files.First();
        string fileExtension = buildOptions.CompileOnly ? ".o" : ".out";

        CompilationUnit compilationUnit = new CompilationUnit(files);

        Dictionary<string, FrontEndResult> frontEndResults = [];
        Dictionary<string, CompilerResult> backEndResults = [];

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            BuildContext buildContext = new(file, compilationUnit, i == 0);
            frontEndResults.Add(file, new Compiler().RunFrontEnd(File.ReadAllText(file), buildOptions, buildContext));
            Console.WriteLine($"Successfully compiled {file}");
        }

        SymbolTable finalSymbolTable = new();
        foreach (FrontEndResult result in frontEndResults.Values)
        {
            finalSymbolTable.Combine(result.SymbolTable);
        }

        foreach ((string file, FrontEndResult result) in frontEndResults)
        {
            BuildContext buildContext = new(file, compilationUnit, file == rootFile);
            backEndResults.Add(file, new Compiler().RunBackEnd(result.AstNodes, finalSymbolTable, buildOptions, buildContext));
        }

        Console.WriteLine("Successfully compiled all files. Writing...");
        if (buildOptions.CompileOnly)
        {
            foreach ((string file, CompilerResult result) in backEndResults)
            {
                string resultPath = Path.Combine(
                    Path.GetDirectoryName(file) ?? "",
                    Path.GetFileNameWithoutExtension(file) + fileExtension
                );

                File.WriteAllText(resultPath, result.Code);
                Console.WriteLine($"Wrote file '{file}' at '{resultPath}'");
            }
        }
        else
        {
            CompilerResult rootFileResult = backEndResults[rootFile];
            List<CompilerResult> otherCompilationUnitResults = backEndResults.Where(x => x.Key != rootFile).Select(x => x.Value).ToList();
            List<string> otherCompilationUnitCode = otherCompilationUnitResults.Select(x => x.Code).ToList();
            string[] imports = backEndResults.SelectMany(x => x.Value.Imports).ToArray();
            string linkerResult = new Linker().Link(rootFileResult.Code, otherCompilationUnitCode, imports);
            File.WriteAllText(buildOptions.OutPath, linkerResult);
            Console.WriteLine($"Executable file at '{buildOptions.OutPath}'");
        }
    }
}
