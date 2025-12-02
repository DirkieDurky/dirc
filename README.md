# DIRC Compiler
This repository contains the compiler for the DIRC Programming Language and compiles from DIRC Programming Language to DIRIC v2.4 assembly.
DIRIC 2.4 is a computer architecture built in Turing Complete. If you own the game, in the schematic hub, look for "DIRIC 2_4 (16-bit)" to try it out.
## Usage
```
dirc sourcePath [flags]
```
The source file should have the `.dirc` file extension.\
If the source path points to a file, the compiler will output a `.out` file with the same name as the `.dirc` file.
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

# Dirc Programming language
A programming language inspired by the C family that compiles to DIRIC assembly.\
Dirc stands for Directly Implemented and Reduced C.

## Declaring local variables
All variables must be declared with an explicit type. The supported types are `int` and `bool`.

To declare an integer variable:
```
int x = 5;
```

To declare a boolean variable:
```
bool flag = true;
```

Later, this variable may be referred to by the given name. This example outs it:
```
outInt(x);
```

## Memory Management and Pointers
Dirc supports pointer types for direct memory manipulation. A pointer type is declared by adding an asterisk (`*`) after the base type.

### Basic Pointer Operations
To declare a pointer variable:
```
int* ptr = 0;
```

To get the address of a variable (address-of operator):
```
int x = 5;
int* ptr = &x;  // ptr now holds the memory address of x
```

To access the value a pointer points to (dereference operator):
```
int y = *ptr;  // y now equals 5
```

You can modify values through pointers:
```
*ptr = 99;  // x now equals 99
```

### Dynamic Memory Allocation
Dirc provides `malloc` and `free` functions for dynamic memory management:

```
import malloc;
import free;

// Allocate memory for 1 integer
int* ptr = malloc(1);  

// Use the allocated memory
*ptr = 42;

// Free the memory when done
free(ptr);
```

The `malloc` function:
- Takes the size (in words) as an argument
- Returns a pointer to the allocated memory

The `free` function:
- Takes a pointer previously returned by malloc
- Deallocates the memory so it can be reused

### Pointer Arithmetic
Pointers support arithmetic operations:

```
int* ptr = malloc(4);  // Allocate space for 4 integers
*(ptr + 0) = 10;       // First integer
*(ptr + 1) = 20;       // Second integer
*(ptr + 2) = 30;       // Third integer
*(ptr + 3) = 40;       // Fourth integer
```

Array-style syntax can also be used with pointers:
```
ptr[0] = 10;  // Equivalent to *(ptr + 0)
ptr[1] = 20;  // Equivalent to *(ptr + 1)
```

## Arrays
Arrays can be declared with a fixed size. These are allocated on the stack.

To declare an integer array (with space for 10 elements):
```
int nums[10];
```

To declare a boolean array (with space for 5 elements):
```
bool flags[5];
```

Arrays can be initialized with values:
```
int nums[5] = {1, 2, 3, 4, 5};
```
or like this:
```
int[] nums = {1, 2, 3, 4, 5};
```
or like this:
```
int* nums = {1, 2, 3, 4, 5};
```

Array elements can be accessed and modified using square bracket notation:
```
nums[2] = 42;        // Set element at index 2 to 42
int x = nums[2];     // Get element at index 2
outInt(nums[0]);      // Print the first element
```

Array indices must be integers and are zero-based.

Arrays can also contain other arrays, creating multidimensional arrays:
```
int[][] twoDArray = {
    {1, 2, 3},
    {4, 5, 6},
    {7, 8, 9}
};

int[] subArray = twoDArray[2];
println(intToString(subArray[1]));
```
This example gets the 3rd array out of twoDArray and then the 2nd element out of that array, so this should result '8'.

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

This example does a bitwise or operation on 'a' and 'b' and outs it.
```
outInt(a | b)
```

To make operations like these easier to deal with, binary and hexadecimal literals are also allowed using the '0b' and '0x' prefixes respectively:
```
outInt(0b01000000 | 0b01000100);
```

Hexadecimal is done using the '0x' prefix:
```
outInt(0x0d ^ 0xd0);
```

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
- `outInt(num)`
Sends the given value to the out register.
- `malloc(size)`
Allocates the specified number of words in memory and returns a pointer to the allocated memory.
- `free(ptr)`
Deallocates memory previously allocated with malloc. The same pointer you got from malloc should be used here. Allocated memory should always be freed to prevent memory leaks.

Functions (both standard and custom ones) may be called as such:
```
outInt(42);
```
Before using a function from the standard library it should be imported using the import statement:
```
import [function];
```

Example:
```
import out;
```


## Creating custom functions
All functions must be declared with an explicit return type and explicit types for all parameters. The supported types are `int`, `bool`, and `void` (for functions that do not return a value).

Example of a function returning an int:
```
int add(int x, int y) {
    return x + y;
}
```

Example of a function returning a bool:
```
bool isEven(int x) {
    return x % 2 == 0;
}
```

Example of a function returning nothing:
```
void outSum(int x, int y) {
    outInt(x + y);
}
```


## If statements
If statements may be used to include a section of code that should only run when a condition is true.\
An if statement takes a boolean value to determine whether the code runs or not.
An if statement might look like this:
```
int x = 4;
int y = 5;
bool cond = (x != y);
if (cond) {
    outInt(3);
}
```
Or directly:
```
if (x == y) {
    outInt(3);
}
```
All comparison operations return a boolean value.


## While loops
While loops repeatedly execute a block of code as long as a specified condition remains true. The syntax is as follows:
```
while (condition) {
    // code to execute while condition is true
}
```
While loops require a boolean condition. The condition must be of type `bool`.

For example:
```
int i = 0;
while (i < 5) {
    outInt(i);
    i++;
}
```
The condition is evaluated before each iteration, meaning the loop body might not execute at all if the condition is false initially.


## For loops
For loops require a boolean condition as the second component. The syntax is as follows:
```
for (initialization; condition; increment) {
    // code to execute
}
```

Example:
```
for (int x = 0; x < 5; x++) {
    outInt(x);
}
```

The condition must be of type `bool`. All three components of the for loop are optional. An infinite loop can be written as:
```
for (;;) {
    outInt(42);
}
```

Each component of the for loop is just an expression. As with other expressions in Dirc, assignments and function calls may be used. This gives the for loop a great deal of flexibility.

The loop variable must be declared with a type if it is declared in the loop header, and can be modified inside the loop body as well.

## Break and Continue Statements

Break and continue statements provide control over loop execution.

### Break Statement
The `break` statement immediately exits the current loop:

```
for (int i = 0; i < 10; i++) {
    if (i == 5) {
        break;  // Exit loop when i equals 5
    }
    outInt(i);
}
```

The `break` statement transfers control to the statement immediately after the loop.

### Continue Statement
The `continue` statement skips the rest of the current loop iteration and jumps to the next iteration:

```
for (int i = 0; i < 5; i++) {
    if (i == 2) {
        continue;  // Skip this iteration
    }
    outInt(i);  // Will not print when i equals 2
}
```

For `while` loops, `continue` jumps back to the condition check:
```
int i = 0;
while (i < 5) {
    i++;
    if (i == 3) {
        continue;
    }
    outInt(i);
}
```

Both `break` and `continue` can only be used inside `while` or `for` loops. Using them outside a loop will result in a compiler error.

## Expressions
An expression is anything that returns a value. Expressions have a type, such as `int` or `bool`. Expressions can be used in other elements of the code, like:
- Function calls (`outInt([expression])`)
- Variable assignments (`x = [expression]`)
- If statements (`if ([bool expression])`)

The following are all valid expressions:
- `4` (returns an int)
- `true` (returns a bool)
- `x` (returns the value of x, with its declared type)
- `x + 5` (returns an int)
- `x == y` (returns a bool)

Function calls and variable assignments are also expressions and have a type. For example, `input()` returns an int, and `x = 4` returns an int.

This means that even function calls or variable assignments can be used where an expression is expected, like the examples given at the start of this subchapter. For example, a function call can be used as an argument for another function call:
```
outInt(input());
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
outInt(4);
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
- `store value address _`
Writes a value to the specified address in memory.
- `load address _ reg`
Copies the value at the specified address in memory to the register specified by the return address.
- `noop _ _ _`
Does nothing at all for a tick.
- `read _ _ reg`
Waits until an input from the user's keyboard is received.
When an input is received, writes it to the specified register
- `file fileNum fileOffset reg`
Reads 8 bytes from a file and writes it to the specified register.
fileNum specifies the file to read from. Currently the only possible value here is 0 because there is only 1 file to read from.
fileOffset specifies the start of where to read from in the file. We will read byte fileOffset to fileOffset + 7 (8 bytes total).

# ABI

Registers `r0-r3` are used to pass arguments to functions.\
Register `r0` is used for return values out of functions.

Currently all registers (`r0-r10`) are "caller-saved".\
In the future, this could behave more like other compilers where:\
Registers `r0-r5` are used as "caller-saved" registers.\
Registers `r6-r10` are used as "callee-saved" registers.

RAM is divided as follows:\
The first 16 words are reserved for global variables\
The first byte (byte 0) of this area holds the current screen pointer; the next position to write to when we're printing. (the cursor if you will).
Byte 1 holds the pointer to the head of the free list.
The free list is stored in all free blocks on the heap,
so it will simply point to the first free block.
Byte 2 holds the screen buffer offset. The number that indicates the offset of the console component. Is used for scrolling the text on the console.

The last 2MiB is for the stack. It starts at the end and grows downward.

The rest is for the heap. It starts after the global area and may grow until it hits the stack.

This may all be changed in Dirc/BuildEnvironment.cs

Memory allocation work as follows:\
The heap is divided up into blocks.\
Each block contains:
- A header that gives some information about that block
- The data that the user can access
- If it's free: A footer that represents the size of the block. (used for coalescing blocks)

The header contains:
- A bool indicating whether it is in use or free. (1 == in use, 0 is free)
- A size that represents how big the block is.
- If it's free: a pointer to the next free block.
- If it's free: a pointer to the previous free block.
