namespace Dirc.Compiling.CodeGen;

public class DirectVariable(string name, int value) : Variable(name)
{
    public int Value = value;
}
