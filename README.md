# Dirc Programming language
A programming language inspired by the C family that compiles to DIRIC assembly.\
Dirc stands for Directly Implemented and Reduced C.

## Declaring local variables
Local variables are declared as such:
```
x = 5;
```
The same syntax can be used to reassign a new value to the variable.

Optionally, the "var" keyword may be used to make extra clear to a reader that a variable is being declared:
```
var x = 5;
```

Later, this variable may be referred to by the given name. This example prints it.
```
print(x);
```

## Calculations
Calculations can be done using the declared variables as follows:
```
x + y
```
But this in itself doesn't do anything. To assign the result to a new variable:
```
z = x + y;
```

The following operations are permitted:
- Addition (+)
- Subtraction (-)
- Bitwise and (&)
- Bitwise or (|)
- Bitwise xor (^)

### Shorthands
Shorthands for all of the above calculations are allowed as well.
This example will add 2 to x:
```
x += 2;
```
The "++" operator can be used to add exactly 1 to a number:
```
x++;
```

## Standard Library
The standard library contains the following functions:
- `input()`
Returns the current input of the program.
- `print(num)`
Send the given input to the out register.
Functions (both standard and custom ones) may be called as such:
```
print(42);
```
Before using a function from the standard library it should be imported using the import statement:
```
import [function];
```

Example:
```
import print;
```

## Creating custom functions
Custom functions are created like the following example:
```
add(x, y) {
    return x + y;
}
```
Optionally, the "function" keyword may be used to make extra clear to a reader that a function is being declared:
```
function add(x, y) {
    return x + y;
}
```

Another example:
```
print(x | 4);
```
To make operations like these easier to deal with, binary and hexadecimal literals are also allowed using the '0b' and '0x' prefixes respectively:
```
print(0b01000000 | 0b01000100);
```

Hexadecimal is done using the '0x' prefix:
```
print(0x0d ^ 0xd0);
```

Functions can optionally return values.
This is done by adding a return statement:
```
return x;
```
This returns the specified value and exits out of the function.
This value can then be used as result of the function:
```
myAdd(x, y) {
    return x + y;
}

print(myAdd(1, 2));
```

## If statements
If statements may be used to include a section of code that should only run when a condition is true.\
An if statement might look like this:
```
if (1 == 2) {
    print(3);
}
```
This if statements prints "3" if 1 is equal to 2.

An if statement can also include expressions like variables:
```
x = 4;
y = 5;

if (x != y) {
    print(3);
}
```
This statement prints 3 if x is not equal to y, which returns 2; x does in fact dot equal y because x = 4 and y = 5.

## While loops
While loops repeatedly execute a block of code as long as a specified condition remains true. The syntax is as follows:
```
while (condition) {
    // code to execute while condition is true
}
```

For example:
```
i = 0;

while (x < 5) {
    print(i);
    i++;
}
```

This loop will print the numbers 0 through 4. On each iteration, i is incremented until it is equal to 5, at which point the condition becomes false and the loop stops.

The condition is evaluated before each iteration, meaning the loop body might not execute at all if the condition is false initially.

## For loops
For loops are typically used when the number of iterations is known in advance. They allow for concise initialization, condition, and increment expressions in a single line. The syntax is as follows:
```
for (initialization; condition; increment) {
    // code to execute
}
```

For example:
```
for (x = 0; x < 5; x++) {
    print(x);
}
```

This loop behaves the same as the while loop example above.\
All three components of the for loop are optional. An infinite loop can be written as:
```
for (;;) {
    print(42);
}
```

Each component of the for loop is just an expression. As with other expressions in Dirc, assignments and function calls may be used. This gives the for loop a great deal of flexibility.

The loop variable does not have to be declared within the loop header, and can be modified inside the loop body as well.

## Expressions
An expression is anything that returns something. Expressions can be used in other elements of the code, like:
- Function calls (print([expression]))
- Variable assignments (x = [expression])
- If statements (if ([expression] != [expression]))

Expression is a very broad term, which means a lot of things can be an expression. The following examples are all expressions:
- `4` (returns 4)
- `4 + 5` (returns 9)
- `x` (returns the value of x)

What can be surprising is that the following examples are also expressions:
- Function calls (like `input()` returns the input)
- Variable assignments (`x = 4` (returns 4))

This means that even function calls or variable assignments can be used where an expression is expected, like the examples given at the start of this subchapter.\
For example, a function call can be used as an argument for another function call:
```
print(input());
```

## Comments
Comments are used to document the code and are ignored by the compiler.

Single-line comments begin with "//". Everything after "//" on that line is treated as a comment:
```
// This is a single-line comment
x = 4; // This is also a single-line comment
```
Multi-line comments are enclosed between /* and */. They can span multiple lines:
```
/*
This is a multi-line comment.
It can be used to describe logic over several lines.
*/
print(4);
```
Comments are useful for explaining complex logic or temporarily disabling code during development.

# DIRIC assembly
DIRIC assembly is a simple assembly language made for the DIRIC ISA which is made in the game Turing Complete.\
DIRIC stands for Directly Implemented Reduced Instruction-set Computer.
This documentation is written for DIRIC v2.3.

## Registers
The DIRIC computer has 16 registers.\
The registers can be referred to in assembly by the following names:
- `r0`
- `r1`
- `r2`
- `r3`
- `r4`
- `r5`
- `r6`
- `r7`
- `r8`
- `r9`
- `r10`
- `fp`
- `sp`
- `lr`
- `pc`
- `in` / `out`

Register `r0-r10` can simply be used to store values.\
Register `fp` is used by the computer as any of the previous registers. This register can be used to store the current "frame pointer" which points to the beginning of the current scope on the stack.\
Register `sp` is used by the computer as the "stack pointer". It always points to the top of the stack.\
Register `lr` is used by the computer to store the byte to continue executing from after a function ends. See `call` and `return` operations.\
Note that this register will need to be pushed and popped to and from the stack manually for nested functions.\
Register `pc` is used by the computer to keep track of the current byte we should read from the program.\
The computer will always read the byte at the index in the pc register and the following 3, thus reading a total of 4 bytes each tick.\
Each tick, the pc will automatically advance by 4 in order to read the next 4 bytes.\
The `pc` register can be overwritten in order to jump.\
The "io" register allows reading from the input when used as operand 1 or 2, and will write to the output when used as result address.

## Comments
`# Comment`
Comments will be ignored by the computer.

## Labels
`label labelname`
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
`jump byte/label _ pc`\
An alias of `mov|i1`. Jumps to the byte or label specified by operand 1 by writing it's value to pc.
The result address should always be `pc`.

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
This repository contains the compiler for the DIRC Programming Language and compiles from DIRC Programming Language to DIRIC v2.1 assembly.

Registers `r0-r3` are used to pass arguments to functions.\
Register `r0` is used for return values out of functions.\
Registers `r0-r5` are used as "caller-saved" registers.\
Registers `r6-r10` are used as "callee-saved" registers.
## Usage
```
dirc sourcePath [flags]
```
The source file should have the `.dirc` file extension.\
If the source path points to a file, the compiler will output a `.diric` file with the same name as the `.dirc` file.
If the source path points to a folder, the compiler will compile all `.dirc` files in the folder and put them in a `builds` folder in the source folder.

### Flags
The following flags can be used by the compiler:
- `--help` or `-h`
Used to view compiler usage.
- `--debug`
Used to set the amount of debug logging you would like to receive while compiling. For more information, see the Debug chapter below.

#### Debug
The debug flag takes a comma-separated array of any of the following options:
- `all`
Logs all types of debugging.
- `general`
Logs general information, such as the stage the compiler is in.
- `lexer`
Logs the output of the lexer.
- `parser`
Logs the output of the parser.
- `allocator`
Logs when registers are allocated and freed by the allocator.
- `stack-trace`
Normally when an exception occurs only the stack trace of the source code is shown.\
Use this option enabled to show the stack trace of the compiler as well.
