namespace Dirc.Compiling.CodeGen;

public class StackStoredVariable(string name, int framePointerOffset, bool isArray = false) : Variable(name)
{
    public int FramePointerOffset = framePointerOffset;
    public bool IsArray = isArray;
}
