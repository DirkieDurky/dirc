namespace Dirc.MetaFile;

public class Function
{
    public required string Name { get; set; }
    public required string ReturnType { get; set; }
    public required Param[] Parameters { get; set; }
}
