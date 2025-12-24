jump _start _ pc

label strlen
push lr _ _
push fp _ _
mov sp _ fp
sub|i2 sp 1 sp
sub|i2 fp 1 r1
store|i1 0 r1 _
label _strlen_while0
sub|i2 fp 1 r1
load r1 _ r2
add r0 r2 r3
load r3 _ r1
ifEq|i2 r1 0 _strlen_whileEnd0
sub|i2 fp 1 r1
sub|i2 fp 1 r2
load r2 _ r3
add|i2 r3 1 r2
store r2 r1 _
jump _strlen_while0 _ pc
label _strlen_whileEnd0
sub|i2 fp 1 r1
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
mov|i1 67108864 _ sp # Initialize stack pointer
store|i1|i2 67108864 0 _ # Initialize screen pointer
store|i1|i2 16 1 _ # Initialize free list head
mov sp _ fp # Initialize frame pointer
sub|i2 sp 6 sp
sub|i2 fp 1 r0
push r0 _ _
sub|i2 fp 6 r1
mov r1 _ r2
store|i1 116 r2 _
add|i2 r1 1 r2
store|i1 101 r2 _
add|i2 r1 2 r2
store|i1 115 r2 _
add|i2 r1 3 r2
store|i1 116 r2 _
add|i2 r1 4 r2
store|i1 0 r2 _
mov r1 _ r0
call strlen _ _
mov r0 _ r1
pop _ _ r0
store r1 r0 _
halt _ _ _