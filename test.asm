.global _start

.text
println:
pushq %rbp
movq %rsp, %rbp
subq $128, %rsp
pushq %rdi
movq %rbp, %rax
subq $136, %rax
movq (%rax), %rbx
movq %rbx, %rdi
call print
popq %rdi
pushq %rdi
movq %rbp, %rax
subq $128, %rax
movq %rax, %rbx
movq $10, (%rbx)
leaq 1(%rax), %rbx
movq $0, (%rbx)
movq %rax, %rdi
call print
popq %rdi
movq %rbp, %rsp
popq %rbp
ret

print:
pushq %rbp
movq %rsp, %rbp
subq $128, %rsp
pushq %rdi
movq $1, %rdi
movq %rbp, %rax
subq $136, %rax
movq (%rax), %rbx
movq %rbx, %rsi
pushq %rbx
pushq %rsi
pushq %rdi
movq %rbp, %rax
subq $136, %rax
movq (%rax), %rcx
movq %rcx, %rdi
call strlen
popq %rdi
popq %rsi
popq %rbx
movq %rax, %rdx
call write
popq %rdi
movq %rbp, %rsp
popq %rbp
ret

strlen:
pushq %rbp
movq %rsp, %rbp
subq $128, %rsp
movq %rbp, %rax
subq $64, %rax
movq $0, (%rax)
_strlen_while0:
movq %rbp, %rax
subq $64, %rax
movq (%rax), %rbx
leaq (%rdi, %rbx), %rcx
movq (%rcx), %rax
cmpq $0, %rax
je _strlen_whileEnd0
movq %rbp, %rax
subq $64, %rax
movq %rbp, %rbx
subq $64, %rbx
movq (%rbx), %rcx
leaq 1(%rcx), %rbx
movq %rbx, (%rax)
jmp _strlen_while0
_strlen_whileEnd0:
movq %rbp, %rax
subq $64, %rax
movq (%rax), %rbx
movq %rbx, %rax
movq %rbp, %rsp
popq %rbp
ret
movq %rbp, %rsp
popq %rbp
ret

write:
mov $1, %rax
syscall
ret

_start:
movq %rsp, %rbp
subq $128, %rsp
movq %rbp, %rax
subq $64, %rax
movq $48, (%rax)
movq %rbp, %rax
subq $128, %rax
movq $5, (%rax)
movq %rbp, %rax
subq $192, %rax
movq $7, (%rax)
movq %rbp, %rax
subq $384, %rax
movq %rax, %rcx
movq %rbp, %rax
subq $128, %rax
movq (%rax), %rdx
movq %rbp, %rax
subq $64, %rax
movq (%rax), %rsi
leaq (%rdx, %rsi), %rax
movq %rax, (%rcx)
movq %rbp, %rax
subq $384, %rax
leaq 1(%rax), %rcx
movq %rbp, %rax
subq $192, %rax
movq (%rax), %rdx
movq %rbp, %rax
subq $64, %rax
movq (%rax), %rsi
leaq (%rdx, %rsi), %rax
movq %rax, (%rcx)
movq %rbp, %rax
subq $384, %rax
leaq 2(%rax), %rcx
movq $0, (%rcx)
movq %rbp, %rax
subq $384, %rax
movq %rax, %rdi
call println
mov $60, %rax
xor %rdi, %rdi
syscall
