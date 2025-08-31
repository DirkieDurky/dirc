namespace Dirc;

public class BuildContext
{
    public string CurrentFilePath { get; }

    public BuildContext(string filePath)
    {
        CurrentFilePath = filePath;
    }
}
