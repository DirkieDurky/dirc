# DIRC Programming language
A programming language inspired by the C family that compiles to DIRIC assembly.\
DIRC stands for Directly Implemented and Reduced C.

# DIRIC assembly
DIRIC assembly is a simple assembly language made for the DIRIC ISA which is made in the game Turing Complete.\
DIRIC stands for Directly Implemented Reduced Instruction Computer.
This documentation is written for DIRIC v2.0.

## Registers
The DIRIC computer has 8 registers.\
The registers can be referred to in assembly by the following names:
- `r0`
- `r1`
- `r2`
- `r3`
- `r4`
- `lr`
- `cnt`
- `in` / `out`

Register `r0-r4` can simply be used to store values.\
Register `lr` is used by the computer to store the byte to continue executing from after a function ends. See `call` and `return` operations.\
Note that this register will need to be pushed and popped to and from the stack manually for nested functions.\
Register `cnt` is used by the computer to keep track of the current byte we should read from the program.\
The computer will always read the byte at the index in the cnt register and the following 3, thus reading a total of 4 bytes each tick.\
Each tick, the cnt will automatically advance by 4 in order to read the next 4 bytes.\
The `cnt` register can be overwritten in order to jump.\
The "io" register allows reading from the input when used as operand 1 or 2, and will write to the output when used as result address.

## Labels
Labels are used to make a byte (or line of assembly code) identifiable by a name such that it's easier to reference later.
Can be used to jump to using the `jump` operation or any of the conditions.
Labels are also used to define the start of functions.
Note: Because labels are not operations they don't need to consist of exactly 4 parts as specified for operations below.

## Operations
DIRIC assembly reads 4 bytes of a program each tick. These bytes are interpreted as follows:\
Byte 0: Opcode\
Byte 1: Operand 1\
Byte 2: Operand 2\
Byte 3: Result address\
All lines in a DIRIC assembly program should be exactly 4 bytes.\
That means that bytes that end up unused should be indicated by a `_`.

The operations the computer supports can be divided in 3 main categories:
- Basic
- Calculations
- Conditions
- Other

Important to know is how operand 1 and 2 are interpreted in different scenarios.\
By default operand 1 and 2 are interpreted as a register address, whose contents will be used for the calculation.\
The interpretation can be changed to interpret as immediate value by changing the first 2 bits of the opcode.\
This can be done by appending "|i1" or "|i2" to the opcode to change the interpretation of operator 1 and operator 2 respectively.\
Other operations define their own definitions of these operands.

### Basic
`mov imm/reg _ reg`\
Moves a value from the source to the destination.\
Operand 2 is unused so should be filled in by `_`.\
`copy imm/reg _ reg`\
An alias for `mov`.\
`jump byte/label _ cnt`\
An alias of `mov|i1`. Jumps to the byte or label specified by operand 1 by writing it's value to cnt.
The result address should always be `cnt`.

### Calculations
The computer allows the following calculations: (Corresponding opcode are shown in parentheses)
- Addition (add)
- Subtraction (sub)
- Bitwise and (and)
- Bitwise or (or)
- Bitwise not (not)
- Bitwise xor (xor)

### Conditions
The computer allows for conditions that can compare two given values and will jump to the location specified in byte 3 if the condition returns true.\
The computer allows for 6 conditions. These conditions return true if: (Corresponding opcode are shown in parentheses)
- Operand 1 equals operand 2 (ifEq)
- Operand 1 doesn't equal operand 2 (ifNotEq)
- Operand 1 is less than operand 2 (ifLess)
- Operand 1 is less than or equal to operand 2 (ifLessOrEq)
- Operand 1 is more than operand 2 (ifMore)
- Operand 1 is more than or equal to operand 2 (ifMoreOrEq)

### Other
The remaining operations the computer allows are as follows:

- `call label _ _`
To call a function. Label is the label of the function that requires jumping to.\
This is different from the `jump` operation because it writes the byte to jump to when the function ends to the `lr` register.\
The `lr` register can then be used by the `return` operation.
- `return _ _ _`
Every function should end with a `return` statement.
Reads from the `lr` register and jumps to that byte in the program.\
- `push value _ _`
Writes a value to the stack and decreases the stack pointer by 1.
- `pop _ _ reg`
Copies the value at the stack pointer to the register specified by the return address and increases the stack pointer by 1.
- `store value location _`
Writes a value to the specified location in memory.
- `load location _ reg`
Copies the value at the specified location in memory to the register specified by the return address.
- `noop _ _ _`
Does nothing at all for a tick.

# DIRC Compiler
This repository contains the compiler for the DIRC Programming Language and compiles from DIRC Programming Language to DIRIC v1.5 assembly.

## Usage
```
dirc filename
```
The file should have the `.dirc` file extension.\
The compiler will output a `.diric` file with the same name as the `.d` file
