.global _start

.text
print:
pushq %rbp
movq %rsp, %rbp
pushq %rdi
movq $1, %rdi
movq %rbp, %rax
subq $8, %rax
movq (%rax), %rbx
movq %rbx, %rsi
movq $20, %rdx
call write
popq %rdi
movq %rbp, %rsp
popq %rbp
ret

write:
mov $1, %rax
syscall
ret

_start:
movq %rsp, %rbp
subq $48, %rsp
movq %rbp, %rax
subq $48, %rax
movq %rax, %rbx
movq $116, (%rbx)
leaq 1(%rax), %rbx
movq $101, (%rbx)
leaq 2(%rax), %rbx
movq $115, (%rbx)
leaq 3(%rax), %rbx
movq $116, (%rbx)
leaq 4(%rax), %rbx
movq $10, (%rbx)
leaq 5(%rax), %rbx
movq $0, (%rbx)
movq %rax, %rdi
call print
mov $60, %rax
xor %rdi, %rdi
syscall
