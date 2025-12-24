jump _start _ pc

label print
push lr _ _
push fp _ _
mov sp _ fp
sub|i2 sp 1 sp
sub|i2 fp 1 r1
store|i1 0 r1 _
label _print_while0
sub|i2 fp 1 r1
load r1 _ r2
add r0 r2 r3
load r3 _ r1
ifEq|i2 r1 0 _print_whileEnd0
sub|i2 fp 1 r1
load r1 _ r2
add r0 r2 r3
load r3 _ r1
ifNotEq|i2 r1 10 _print_else0
push r0 _ _
call printNewline _ _
mov r0 _ r1
pop _ _ r0
jump _print_ifElseEnd0 _ pc
label _print_else0
push r0 _ _
sub|i2 fp 2 r1
load r1 _ r2
sub|i2 fp 1 r1
load r1 _ r3
add r2 r3 r4
load r4 _ r1
mov r1 _ r0
call printChar _ _
mov r0 _ r1
pop _ _ r0
label _print_ifElseEnd0
label _print_while1
sub|i2 fp 1 r1
sub|i2 fp 1 r2
load r2 _ r3
add|i2 r3 1 r2
store r2 r1 _
jump _print_while0 _ pc
label _print_whileEnd0
mov fp _ sp
pop _ _ fp
pop _ _ lr
return _ _ _

label printNewline
load|i1 0 _ r0
sub|i2 r0 67108864 r0
mod|i2 r0 80 r1
sub r0 r1 r0
add|i2 r0 80 r0
add|i2 r0 67108864 r0
store|i2 r0 0 _
return _ _ _

label printChar
load|i1 0 _ r1
store r0 r1 _
add|i2 r1 1 r1
store|i2 r1 0 _
return _ _ _

label _start
mov|i1 67108864 _ sp # Initialize stack pointer
store|i1|i2 67108864 0 _ # Initialize screen pointer
store|i1|i2 16 1 _ # Initialize free list head
mov sp _ fp # Initialize frame pointer
sub|i2 sp 3 sp
sub|i2 fp 1 r0
store|i1 5 r0 _
sub|i2 fp 3 r0
mov r0 _ r2
store|i1 5 + 48 r2 _
sub|i2 fp 3 r0
add|i2 r0 1 r2
store|i1 0 r2 _
sub|i2 fp 3 r0
call print _ _
halt _ _ _