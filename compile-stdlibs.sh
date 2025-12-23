#!/bin/zsh

echo "stdlibcore-x86:"
./dirc ./Dirc/HAL/X86/stdlibcore-x86/**/*.dirc -m stdlibcore-x86 -o ./Dirc/HAL/X86/stdlibcore-x86/Compiled --ignore-stdlib --arch=x86
echo "stdlibcore-diric:"
./dirc ./Dirc/HAL/Diric/stdlibcore-diric/**/*.dirc -m stdlibcore-diric -o ./Dirc/HAL/Diric/stdlibcore-diric/Compiled --ignore-stdlib
echo "stdlib-x86:"
./dirc ./Dirc/Libraries/Standard/**/*.dirc -m stdlib-x86 -o ./Dirc/HAL/X86/stdlib-x86 --ignore-stdlib --arch=x86
echo "stdlib-diric:"
./dirc ./Dirc/Libraries/Standard/**/*.dirc -m stdlib-diric -o ./Dirc/HAL/Diric/stdlib-diric --ignore-stdlib
