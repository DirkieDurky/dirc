using System.Text;
using Dirc.Compiling;
using Dirc.Compiling.CodeGen;
using Dirc.Compiling.CodeGen.Allocating;
using Dirc.Compiling.Semantic;

namespace Dirc.HAL;

public interface ITargetArchitecture
{
    RegisterBase[] AvailableRegisters { get; }
    RegisterBase StackPointerRegister { get; }
    RegisterBase FramePointerRegister { get; }
    RegisterBase? LinkRegister { get; }
    RegisterBase[] ArgumentRegisters { get; } // Registers used to pass arguments to functions
    RegisterBase ReturnRegister { get; } // Register used for the return value of functions
    RegisterBase[] CallerSavedRegisters { get; }
    RegisterBase[] CalleeSavedRegisters { get; }
    int StackAlignment { get; } // The amount of bytes the stack pointer has to be aligned by before each function call
    int StackSlotSize { get; } // The amount of bytes the stack pointer moves with every push / pop
    string StdlibName { get; }
    string StdlibcoreName { get; }
    IRuntimeLibrary RuntimeLibrary { get; }
    ICodeGenBase CodeGenBase { get; }
    Dictionary<string, int> Keywords { get; }
    string Headers { get; } // The text the linker should put at the start of the file
    string StartLabel { get; }
}

public interface ICodeGenBase
{
    public StringBuilder Code { get; }
    void EmitLabel(string name);
    void EmitComment(string comment);
    void EmitMov(IOperand item, Register result);
    void EmitJump(string label);
    void EmitBinaryOperation(Operation op, IOperand left, IOperand? right, Register result);
    void EmitIf(Comparer cond, IOperand left, IOperand right, string result);
    void EmitFunctionCall(string label, bool unresolved);
    void EmitReturn(bool final = true);
    void EmitPush(IOperand value);
    void EmitPop(Register result);
    void EmitStore(IOperand value, IOperand address);
    void EmitLoad(IOperand location, Register result);
    void EmitNoop();
    void EmitHalt();
    void EmitExit();
    void EmitEmptyLine();
    void Emit(string assembly);
}

public interface IRuntimeLibrary
{
    bool HasFunction(string name);
    FunctionSignature GetFunctionSignature(string name);
    Dictionary<string, FunctionSignature> GetAllFunctionSignatures();
    string GetFunction(string name);
    string GetPath();
    string GetName();
}
