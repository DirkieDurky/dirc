using Dirc.Compiling.Semantic;

namespace Dirc;

public class BuildContext(string filePath, CompilationUnit compilationUnit, bool isRootFile)
{
    public string CurrentFilePath { get; } = filePath;
    public CompilationUnit CompilationUnit { get; } = compilationUnit;
    // Root file is the only file in a compilation unit that may contain top-level code and will be the starting point
    public bool IsRootFile { get; set; } = isRootFile;
}

public class CompilationUnit(List<string> filePaths)
{
    public List<string> FilePaths { get; } = filePaths;
}
