namespace Dirc.Compiling;

public class SymbolTable
{
    public List<MetaFile.Function> FunctionTable { get; set; } = [];

    public void Combine(SymbolTable other)
    {
        FunctionTable.AddRange(other.FunctionTable);
    }
}
