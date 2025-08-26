public class CompilerContext
{
    public string CurrentFilePath { get; }

    public CompilerContext(string filePath)
    {
        CurrentFilePath = filePath;
    }
}
