namespace Dirc.Compiling.CodeGen;

public class Variable : Symbol
{
    public int FramePointerOffset { get; }
    public bool IsArray { get; }

    public Variable(string name, int framePointerOffset, bool isArray = false) : base(name)
    {
        FramePointerOffset = framePointerOffset;
        IsArray = isArray;
    }
}
