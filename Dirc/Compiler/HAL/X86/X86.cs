namespace Dirc.HAL;

public class X86 : ITargetArchitecture
{
    public RegisterBase[] AvailableRegisters => [
            new RegisterBase(0, "r0", false),
            new RegisterBase(1, "r1", false),
            new RegisterBase(2, "r2", false),
            new RegisterBase(3, "r3", false),
            new RegisterBase(4, "r4", false),
            new RegisterBase(5, "r5", false),
            new RegisterBase(6, "r6", false),
            new RegisterBase(7, "r7", false),
            new RegisterBase(8, "r8", false),
            new RegisterBase(9, "r9", false),
            new RegisterBase(10, "r10", false),
            new RegisterBase(11, "sp", true),
            new RegisterBase(12, "fp", true),
            new RegisterBase(13, "lr", true),
        ];

    public RegisterBase StackPointerRegister => new RegisterBase(11, "sp", true);

    public RegisterBase FramePointerRegister => new RegisterBase(12, "fp", true);

    public RegisterBase LinkRegister => new RegisterBase(13, "lr", true);

    public RegisterBase[] ArgumentRegisters => [
        new RegisterBase(0, "r0", false),
        new RegisterBase(1, "r1", false),
        new RegisterBase(2, "r2", false),
        new RegisterBase(3, "r3", false),
    ];

    public RegisterBase ReturnRegister => new RegisterBase(0, "r0", false);

    public RegisterBase[] CallerSavedRegisters => [
            new RegisterBase(0, "r0", false),
            new RegisterBase(1, "r1", false),
            new RegisterBase(2, "r2", false),
            new RegisterBase(3, "r3", false),
            new RegisterBase(4, "r4", false),
            new RegisterBase(5, "r5", false),
            new RegisterBase(6, "r6", false),
            new RegisterBase(7, "r7", false),
            new RegisterBase(8, "r8", false),
            new RegisterBase(9, "r9", false),
            new RegisterBase(10, "r10", false),
        ];

    public RegisterBase[] CalleeSavedRegisters => [];

    public int StackAlignment => 1;

    public string StandardCoreLibraryName => "stdlibcore-x86";

    public IRuntimeLibrary RuntimeLibrary => new RuntimeLibraryX86();

    public ICodeGenBase CodeGenBase => new CodeGenBaseX86();

    public Dictionary<string, int> Keywords => new() {
        { "_", 0b00000000 },
        { "add", 0b00000000 },
        { "and", 0b00000010 },
        { "call", 0b00010000 },
        { "copy", 0b00010010 },
        { "fp", 0b00001011 },
        { "i1", 0b10000000 },
        { "i2", 0b01000000 },
        { "ifEq", 0b00100000 },
        { "ifLess", 0b00100010 },
        { "ifLessOrEq", 0b00100011 },
        { "ifMore", 0b00100100 },
        { "ifMoreOrEq", 0b00100101 },
        { "ifNotEq", 0b00100001 },
        { "in", 0b00001111 },
        { "jump", 0b10010010 },
        { "load", 0b00010110 },
        { "lr", 0b00001101 },
        { "mov", 0b00010010 },
        { "noop", 0b00010111 },
        { "read", 0b00011000 },
        { "file", 0b00011001 },
        { "scroll", 0b00011010 },
        { "halt", 0b00011111 },
        { "not", 0b00000100 },
        { "or", 0b00000011 },
        { "out", 0b10001111 },
        { "pc", 0b00001110 },
        { "pop", 0b00010100 },
        { "push", 0b00010011 },
        { "r0", 0b00000000 },
        { "r1", 0b00000001 },
        { "r2", 0b00000010 },
        { "r3", 0b00000011 },
        { "r4", 0b00000100 },
        { "r5", 0b00000101 },
        { "r6", 0b00000110 },
        { "r7", 0b00000111 },
        { "r8", 0b00001000 },
        { "r9", 0b00001001 },
        { "r10", 0b00001010 },
        { "return", 0b00010001 },
        { "sp", 0b00001100 },
        { "store", 0b00010101 },
        { "sub", 0b00000001 },
        { "xor", 0b00000101 },
        { "mul", 0b00000110 },
        { "div", 0b00000111 },
        { "mod", 0b00001000 },
    };
}
