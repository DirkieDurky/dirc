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
pushq %rbx
pushq %rsi
pushq %rdi
movq %rbp, %rax
subq $8, %rax
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
subq $8, %rsp
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
subq $352, %rsp
movq %rbp, %rax
subq $2816, %rax
movq %rax, %rbx
movq $72, (%rbx)
leaq 1(%rax), %rbx
movq $111, (%rbx)
leaq 2(%rax), %rbx
movq $108, (%rbx)
leaq 3(%rax), %rbx
movq $121, (%rbx)
leaq 4(%rax), %rbx
movq $32, (%rbx)
leaq 5(%rax), %rbx
movq $115, (%rbx)
leaq 6(%rax), %rbx
movq $104, (%rbx)
leaq 7(%rax), %rbx
movq $105, (%rbx)
leaq 8(%rax), %rbx
movq $116, (%rbx)
leaq 9(%rax), %rbx
movq $32, (%rbx)
leaq 10(%rax), %rbx
movq $109, (%rbx)
leaq 11(%rax), %rbx
movq $97, (%rbx)
leaq 12(%rax), %rbx
movq $97, (%rbx)
leaq 13(%rax), %rbx
movq $116, (%rbx)
leaq 14(%rax), %rbx
movq $32, (%rbx)
leaq 15(%rax), %rbx
movq $100, (%rbx)
leaq 16(%rax), %rbx
movq $105, (%rbx)
leaq 17(%rax), %rbx
movq $116, (%rbx)
leaq 18(%rax), %rbx
movq $32, (%rbx)
leaq 19(%rax), %rbx
movq $119, (%rbx)
leaq 20(%rax), %rbx
movq $101, (%rbx)
leaq 21(%rax), %rbx
movq $114, (%rbx)
leaq 22(%rax), %rbx
movq $107, (%rbx)
leaq 23(%rax), %rbx
movq $116, (%rbx)
leaq 24(%rax), %rbx
movq $32, (%rbx)
leaq 25(%rax), %rbx
movq $97, (%rbx)
leaq 26(%rax), %rbx
movq $108, (%rbx)
leaq 27(%rax), %rbx
movq $108, (%rbx)
leaq 28(%rax), %rbx
movq $101, (%rbx)
leaq 29(%rax), %rbx
movq $109, (%rbx)
leaq 30(%rax), %rbx
movq $97, (%rbx)
leaq 31(%rax), %rbx
movq $97, (%rbx)
leaq 32(%rax), %rbx
movq $108, (%rbx)
leaq 33(%rax), %rbx
movq $32, (%rbx)
leaq 34(%rax), %rbx
movq $103, (%rbx)
leaq 35(%rax), %rbx
movq $101, (%rbx)
leaq 36(%rax), %rbx
movq $119, (%rbx)
leaq 37(%rax), %rbx
movq $111, (%rbx)
leaq 38(%rax), %rbx
movq $111, (%rbx)
leaq 39(%rax), %rbx
movq $110, (%rbx)
leaq 40(%rax), %rbx
movq $63, (%rbx)
leaq 41(%rax), %rbx
movq $33, (%rbx)
leaq 42(%rax), %rbx
movq $10, (%rbx)
leaq 43(%rax), %rbx
movq $0, (%rbx)
movq %rax, %rdi
call print
mov $60, %rax
xor %rdi, %rdi
syscall
