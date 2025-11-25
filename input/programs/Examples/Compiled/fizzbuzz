mov|i1 127 _ sp # Initialize stack pointer
store|i1|i2 256 0 _ # Initialize screen pointer
mov sp _ fp # Initialize frame pointer
jump _start _ pc

label printChar
load|i1 0 _ r1
store r0 r1 _
add|i2 r1 1 r1
store|i2 r1 0 _
return _ _ _

label printNewline
load|i1 0 _ r0
sub|i2 r0 256 r0
mod|i2 r0 80 r1
sub r0 r1 r0
add|i2 r0 80 r0
add|i2 r0 256 r0
store|i2 r0 0 _
return _ _ _

label println
push lr _ _
push fp _ _
mov sp _ fp
sub|i2 sp 1 sp
mov fp _ r1
store|i1 0 r1 _
label _println_while0
mov fp _ r1
load r1 _ r2
add r0 r2 r3
load r3 _ r1
ifEq|i2 r1 0 _println_whileEnd0
sub|i2 fp 1 r1
store r0 r1 _
sub|i2 fp 1 r2
load r2 _ r3
mov fp _ r2
load r2 _ r4
add r3 r4 r5
load r5 _ r2
mov r2 _ r0
call printChar _ _
mov r0 _ r2
sub|i2 fp 1 r3
load r3 _ r0
mov fp _ r2
load r2 _ r3
add|i2 r3 1 r2
mov fp _ r3
store r2 r3 _
jump _println_while0 _ pc
label _println_whileEnd0
sub|i2 fp 1 r1
store r0 r1 _
call printNewline _ _
mov r0 _ r2
sub|i2 fp 1 r3
load r3 _ r0
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _

label print
push lr _ _
push fp _ _
mov sp _ fp
sub|i2 sp 1 sp
mov fp _ r1
store|i1 0 r1 _
label _print_while0
mov fp _ r1
load r1 _ r2
add r0 r2 r3
load r3 _ r1
ifEq|i2 r1 0 _print_whileEnd0
mov fp _ r1
load r1 _ r2
add r0 r2 r3
load r3 _ r1
ifNotEq|i2 r1 10 _print_else0
sub|i2 fp 1 r1
store r0 r1 _
call printNewline _ _
mov r0 _ r2
sub|i2 fp 1 r3
load r3 _ r0
jump _print_ifElseEnd0 _ pc
label _print_else0
sub|i2 fp 1 r1
store r0 r1 _
sub|i2 fp 1 r2
load r2 _ r3
mov fp _ r2
load r2 _ r4
add r3 r4 r5
load r5 _ r2
mov r2 _ r0
call printChar _ _
mov r0 _ r2
sub|i2 fp 1 r3
load r3 _ r0
label _print_ifElseEnd0
mov fp _ r1
load r1 _ r2
add|i2 r2 1 r1
mov fp _ r2
store r1 r2 _
jump _print_while0 _ pc
label _print_whileEnd0
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _

label printInt
push lr _ _
push fp _ _
mov sp _ fp
mov fp _ r1
store r0 r1 _
mov fp _ r2
load r2 _ r3
mov r3 _ r0
call intToString _ _
mov r0 _ r2
mov r2 _ r0
call print _ _
mov r0 _ r2
mov fp _ r3
load r3 _ r0
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _

label intToString
push lr _ _
push fp _ _
mov sp _ fp
sub|i2 sp 22 sp
sub|i2 fp 1 r1
store r0 r1 _
sub|i2 fp 1 r2
load r2 _ r3
mov r3 _ r0
call getDigitCountInInt _ _
mov r0 _ r2
sub|i2 fp 1 r3
load r3 _ r0
mov fp _ r3
store r2 r3 _
mov fp _ r2
load r2 _ r3
sub|i2 r3 1 r2
sub|i2 fp 22 r3
store r2 r3 _
sub|i2 fp 21 r2
sub|i2 fp 22 r3
load r3 _ r4
add|i2 r4 1 r3
add r2 r3 r5
store|i1 0 r5 _
label _printInt_while0
ifLessOrEq|i2 r0 0 _printInt_whileEnd0
sub|i2 fp 21 r2
sub|i2 fp 22 r3
load r3 _ r4
add r2 r4 r5
mod|i2 r0 10 r2
add|i2 r2 48 r4
store r4 r5 _
div|i2 r0 10 r2
mov r2 _ r0
sub|i2 fp 22 r2
load r2 _ r3
sub|i2 r3 1 r2
sub|i2 fp 22 r3
store r2 r3 _
jump _printInt_while0 _ pc
label _printInt_whileEnd0
sub|i2 fp 21 r2
mov r2 _ r0
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _

label getDigitCountInInt
push lr _ _
push fp _ _
mov sp _ fp
sub|i2 sp 1 sp
mov fp _ r1
store|i1 1 r1 _
ifLess|i2 r0 10000000000000000 _printInt_if0
mov fp _ r1
load r1 _ r2
add|i2 r2 16 r1
mov fp _ r2
store r1 r2 _
div|i2 r0 10000000000000000 r1
mov r1 _ r0
label _printInt_if0
ifLess|i2 r0 100000000 _printInt_if1
mov fp _ r1
load r1 _ r2
add|i2 r2 8 r1
mov fp _ r2
store r1 r2 _
div|i2 r0 100000000 r1
mov r1 _ r0
label _printInt_if1
ifLess|i2 r0 10000 _printInt_if2
mov fp _ r1
load r1 _ r2
add|i2 r2 4 r1
mov fp _ r2
store r1 r2 _
div|i2 r0 10000 r1
mov r1 _ r0
label _printInt_if2
ifLess|i2 r0 100 _printInt_if3
mov fp _ r1
load r1 _ r2
add|i2 r2 2 r1
mov fp _ r2
store r1 r2 _
div|i2 r0 100 r1
mov r1 _ r0
label _printInt_if3
ifLess|i2 r0 10 _printInt_if4
mov fp _ r1
load r1 _ r2
add|i2 r2 1 r1
mov fp _ r2
store r1 r2 _
label _printInt_if4
mov fp _ r1
load r1 _ r2
mov r2 _ r0
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _

label _start
sub|i2 sp 21 sp
mov fp _ r0
store|i1 1 r0 _
label _fizzbuzz_while0
mov fp _ r0
load r0 _ r1
ifMoreOrEq|i2 r1 100 _fizzbuzz_whileEnd0
mov fp _ r0
load r0 _ r1
mod|i2 r1 15 r0
ifNotEq|i2 r0 0 _fizzbuzz_else0
sub|i2 fp 10 r0
mov r0 _ r1
store|i1 70 r1 _
add|i2 r0 1 r1
store|i1 105 r1 _
add|i2 r0 2 r1
store|i1 122 r1 _
add|i2 r0 3 r1
store|i1 122 r1 _
add|i2 r0 4 r1
store|i1 98 r1 _
add|i2 r0 5 r1
store|i1 117 r1 _
add|i2 r0 6 r1
store|i1 122 r1 _
add|i2 r0 7 r1
store|i1 122 r1 _
add|i2 r0 8 r1
store|i1 33 r1 _
add|i2 r0 9 r1
store|i1 0 r1 _
call println _ _
jump _fizzbuzz_ifElseEnd0 _ pc
label _fizzbuzz_else0
mov fp _ r0
load r0 _ r1
mod|i2 r1 5 r0
ifNotEq|i2 r0 0 _fizzbuzz_else1
sub|i2 fp 5 r0
mov r0 _ r1
store|i1 66 r1 _
add|i2 r0 1 r1
store|i1 117 r1 _
add|i2 r0 2 r1
store|i1 122 r1 _
add|i2 r0 3 r1
store|i1 122 r1 _
add|i2 r0 4 r1
store|i1 0 r1 _
call println _ _
jump _fizzbuzz_ifElseEnd1 _ pc
label _fizzbuzz_else1
mov fp _ r0
load r0 _ r1
mod|i2 r1 3 r0
ifNotEq|i2 r0 0 _fizzbuzz_else2
sub|i2 fp 5 r0
mov r0 _ r1
store|i1 70 r1 _
add|i2 r0 1 r1
store|i1 105 r1 _
add|i2 r0 2 r1
store|i1 122 r1 _
add|i2 r0 3 r1
store|i1 122 r1 _
add|i2 r0 4 r1
store|i1 0 r1 _
call println _ _
jump _fizzbuzz_ifElseEnd2 _ pc
label _fizzbuzz_else2
mov fp _ r0
load r0 _ r1
mov r1 _ r0
call intToString _ _
call println _ _
label _fizzbuzz_ifElseEnd2
label _fizzbuzz_ifElseEnd1
label _fizzbuzz_ifElseEnd0
mov fp _ r0
load r0 _ r1
add|i2 r1 1 r0
mov fp _ r1
store r0 r1 _
jump _fizzbuzz_while0 _ pc
label _fizzbuzz_whileEnd0