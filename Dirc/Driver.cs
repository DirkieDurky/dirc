using Dirc.Compiling;
using Dirc.Linking;

namespace Dirc;

public class Driver
{
    public void Run(List<string> files, BuildOptions buildOptions)
    {
        string fileExtension = buildOptions.CompileOnly ? ".o" : ".out";

        CompilationUnit compilationUnit = new CompilationUnit(files);

        foreach (string file in files)
        {
            string resultPath = Path.Combine(
                Path.GetDirectoryName(file) ?? "",
                Path.GetFileNameWithoutExtension(file) + fileExtension
            );

            CompilerResult compilerResult = new Compiler().Compile(File.ReadAllText(file), buildOptions, new BuildContext(file, compilationUnit));

            if (buildOptions.CompileOnly)
            {
                File.WriteAllText(resultPath, compilerResult.Code);
                Console.WriteLine($"Successfully compiled {file}. Compiled file at {resultPath}");
            }
            else
            {
                string linkerResult = new Linker().Link(compilerResult.Code, compilerResult.Imports);
                File.WriteAllText(resultPath, linkerResult);
                Console.WriteLine($"Successfully compiled and linked {file}. Executable file at {resultPath}");
            }
        }
    }
}
