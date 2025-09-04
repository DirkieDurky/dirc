namespace Dirc.Compiling;

public class SymbolTable
{
    public List<MetaFile.Function> Functions { get; set; } = [];

    public void Combine(SymbolTable other)
    {
        Functions.AddRange(other.Functions);
    }
}
