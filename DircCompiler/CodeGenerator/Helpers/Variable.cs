namespace DircCompiler.CodeGen;

public class Variable : Symbol
{
    public int FramePointerOffset { get; }

    public Variable(string name, int framePointerOffset) : base(name)
    {
        FramePointerOffset = framePointerOffset;
    }
}
