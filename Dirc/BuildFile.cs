namespace Dirc;

public class BuildFile(BuildContext buildContext, string filePath)
{
    public string FilePath { get; set; } = filePath;
    public BuildContext BuildContext { get; set; } = buildContext;
}
