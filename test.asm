.global _start

.text

println:
push %rbp
movq %rsp, %rbp
push %rdi
movq %rbp, %rax
movq %rax, %rbx
movq %rbx, %rdi
call print
pop %rdi
movq %rbp, %rsp
pop %rbp
ret

print:
push %rbp
movq %rsp, %rbp
push %rdi
movq $1, %rdi
movq %rbp, %rax
movq %rax, %rbx
movq %rbx, %rsi
movq $20, %rdx
call write
pop %rdi
movq %rbp, %rsp
pop %rbp
ret

write:
mov $1, %rax
syscall
ret

_start:
subq $5, %rsp
movq %rbp, %rax
subq $4, %rax
movq %rax, %rbx
movq $116, %rbx
leaq 1(%rax), %rbx
movq $101, %rbx
leaq 2(%rax), %rbx
movq $115, %rbx
leaq 3(%rax), %rbx
movq $116, %rbx
leaq 4(%rax), %rbx
movq $0, %rbx
movq %rax, %rdi
call println
mov $60, %rax
xor %rdi, %rdi
syscall
