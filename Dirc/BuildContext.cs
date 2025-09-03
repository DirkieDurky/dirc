namespace Dirc;

public class BuildContext(string filePath, CompilationUnit compilationUnit)
{
    public string CurrentFilePath { get; } = filePath;
    public CompilationUnit CompilationUnit { get; } = compilationUnit;
}

public class CompilationUnit(List<string> filePaths)
{
    public List<string> FilePaths { get; } = filePaths;
}
