using System.Reflection.Metadata;
using Dirc.Compiling.Semantic;

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

    public const string ObjectFileExtension = "o";

    public const int StackAlignment = 1; // By how many bytes to align the stack

    // Width of data in the computer in bits (how many bit the computer is)
    // This should match the data width used ingame.
    // When changing this, usually changing the data width in the ram component and
    // replacing the condition component by it's counterpart for the new data width is sufficient.
    // public const int DataWidth = 16;
    public const int DataWidth = 64;
    public const int WordSize = DataWidth / 8; // Size of a word in bytes.
    // This is important because you tell the game where to store or load something by specifying the word you want to modify.
    // This means we can't modify one byte directly. We can only change 1 entire word.

    // Size of the ram in bytes. This may not be higher than the size of ram in game
    public const int RamBytes = 536870912; // 512MiB
    // public const int RamBytes = 256; // Fits in RAM display ingame
    // public const int RamBytes = 65536;
    public const int RamWords = RamBytes / WordSize;
    public const int MaxRamAddress = RamWords - 1;

    public const int ScreenBufferStart = MaxRamAddress + 1;

    // Defining how to divide up RAM:
    // Sizes of each section in bytes
    public const int GlobalBytes = 16 * WordSize;
    // public const int StackBytes = 2097152; // 2MiB
    public const int StackBytes = 128;
    public const int HeapBytes = RamBytes - GlobalBytes - StackBytes; // the rest

    // Sizes of each section in words
    public const int GlobalSize = GlobalBytes / WordSize;
    public const int HeapSize = HeapBytes / WordSize;
    public const int StackSize = StackBytes / WordSize;

    // Starts and ends in words
    // These ranges are inclusive, meaning that the end address can be written to but the end address + 1 cannot.
    public const int GlobalStart = 0;
    public const int GlobalEnd = GlobalStart + GlobalSize - 1;
    public const int HeapStart = GlobalEnd + 1;
    public const int HeapEnd = HeapStart + HeapSize - 1;
    public const int StackStart = MaxRamAddress;
    public const int StackEnd = MaxRamAddress - StackSize + 1;

    // Locations of global variables in global section
    public const int ScreenPointerAddress = 0;
    public const int FreeListHeadAddress = 1;
    public const int ScrollOffsetAddress = 2;

    // Define global constants to be exposed to the source language
    public static List<GlobalConstant> GlobalConstants =
    [
        new GlobalConstant("DATA_WIDTH", Int.Instance, DataWidth),
        new GlobalConstant("MAX_INT_VALUE", Int.Instance, (int)(Math.Pow(2, DataWidth) / 2 - 1)),
        new GlobalConstant("MAX_RAM_ADDRESS", Int.Instance, MaxRamAddress),
        new GlobalConstant("SCREEN_BUFFER_START", Int.Instance, ScreenBufferStart),

        new GlobalConstant("GLOBAL_START", Int.Instance, GlobalStart),
        new GlobalConstant("GLOBAL_END", Int.Instance, GlobalEnd),
        new GlobalConstant("GLOBAL_SIZE", Int.Instance, GlobalSize),
        new GlobalConstant("HEAP_START", Int.Instance, HeapStart),
        new GlobalConstant("HEAP_END", Int.Instance, HeapEnd),
        new GlobalConstant("HEAP_SIZE", Int.Instance, HeapSize),
        new GlobalConstant("STACK_START", Int.Instance, StackStart),
        new GlobalConstant("STACK_END", Int.Instance, StackEnd),
        new GlobalConstant("STACK_SIZE", Int.Instance, StackSize),

        new GlobalConstant("SCREEN_POINTER_ADDRESS", Int.Instance, ScreenPointerAddress),
        new GlobalConstant("FREE_LIST_HEAD_ADDRESS", Int.Instance, FreeListHeadAddress),
        new GlobalConstant("SCROLL_OFFSET_ADDRESS", Int.Instance, ScrollOffsetAddress),
    ];
}

public class GlobalConstant(string name, SimpleType type, int value)
{
    public string Name = name;
    public SimpleType Type = type;
    public int Value = value;
}
