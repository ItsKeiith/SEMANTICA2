;Analizador Lexico
;Autor: Kevin Hernández Cuestas
org 100h
MOV AX, 1
PUSH AX
MOV AX, 1
PUSH AX
POP BX
POP AX
CMP AX, BX
JE etiquetaIF1
MOV AX, 3
PUSH AX
MOV AX, 5
PUSH AX
POP BX
POP AX
ADD AX,BX
POP AX
MOV AX, 8
PUSH AX
POP BX
POP AX
MUL BX
PUSH AX
MOV AX, 10
PUSH AX
MOV AX, 4
PUSH AX
POP BX
POP AX
SUB AX,BX
POP AX
MOV AX, 2
PUSH AX
POP BX
POP AX
DIV BX
PUSH BX
POP BX
POP AX
SUB AX,BX
POP AX
POP AX
MOV radio, AX
JMP etiquetaelse1
etiquetaIF1:
etiquetaelse1:
MOV AX, radio
PUSH AX
MOV AX, 10
PUSH AX
POP BX
POP AX
CMP AX, BX
JE etiquetaIF2
JMP etiquetaelse2
etiquetaIF2:
etiquetaelse2:
ret
; Variables: 
x dd 0
radio db 0
pi dq 0
