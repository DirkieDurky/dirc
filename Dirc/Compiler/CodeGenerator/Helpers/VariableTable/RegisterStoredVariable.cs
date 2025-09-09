using Dirc.Compiling.CodeGen.Allocating;

namespace Dirc.Compiling.CodeGen;

class RegisterStoredVariable(string name, Register register) : Variable(name)
{
    public Register Register = register;
}
