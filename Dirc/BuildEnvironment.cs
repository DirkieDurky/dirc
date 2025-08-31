namespace Dirc;

public class BuildEnvironment
{
    public static Dictionary<string, int> AssemblyKeywords { get; } = new() {
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

    public static int DataWidth => 64; // Computer is 64-bit now
                                       // public static int RamBytes { get; } = 536870912; // RAM is 512MiB
    public static int RamBytes => 256; // Fits in RAM display ingame
    public static int ScreenBufferStart => RamBytes;
    public static int MaxRamValue => (RamBytes - 1) / 8;

    public static int HeapStart => 16;

    public static int ScreenPointerAddress => 1;
}
