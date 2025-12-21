using Dirc.Compiling.Semantic;

namespace Dirc;

public class BuildEnvironment
{
    public const string ObjectFileExtension = "o";

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
