public class CompilerContext
{
    public string CurrentFilePath { get; }
    private readonly Stack<(string, int)> _callStack = new();

    public CompilerContext(string filePath)
    {
        CurrentFilePath = filePath;
    }

    public void PushCall(string filename, int line)
    {
        _callStack.Push((filename, line));
    }

    public void PopCall()
    {
        _callStack.Pop();
    }

    public IEnumerable<(string, int)> GetCallStack()
    {
        return _callStack.Reverse();
    }
}
