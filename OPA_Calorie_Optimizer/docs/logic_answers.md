# Respuestas - Razonamiento lógico

> Basado en la hoja del PDF (página 1).

## Ejercicio 1: completar la secuencia

1) `RQP, ONM, LKJ, ____, FED`
- Patrón: cada bloque es 3 letras descendentes y la primera letra baja de 3 en 3: R→O→L→I→F
- Respuesta: **IHG**

2) `KBJ, LCK, MDL, NEM, ____`
- Patrón por posición: (K,L,M,N,...) (B,C,D,E,...) (J,K,L,M,...)
- Respuesta: **OFN**

3) `104, 109, 115, 122, 130, ____`
- Diferencias: +5, +6, +7, +8, +9
- Respuesta: **139**

4) `15, 31, 63, 127, 255, ____`
- Patrón: *2 + 1
- Respuesta: **511**

---

## Ejercicio 2: ¿Desea beber algo?

Personas: A, B, C, D, E

Datos:
- Roles: artista, médico, periodista, deportista, juez
- Bebida: té o café
- A, C y el juez prefieren té
- B y el periodista prefieren café
- El deportista, D y A son amigos, pero 2 de ellos prefieren café
- El artista es hermano de C

### Deducción (interpretación estándar)
1) A prefiere té.
2) C prefiere té.
3) B prefiere café.
4) Periodista prefiere café.
5) Como A (en el grupo A-D-deportista) toma té, los otros 2 deben tomar café ⇒ D café y deportista café.
6) El juez toma té. Como ya tenemos A y C con té y D/B con café, el candidato natural para juez es E ⇒ **E = juez**.
7) Deportista debe ser café y no puede ser C (té) ⇒ **B = deportista**.
8) Periodista toma café y ya B es deportista ⇒ **D = periodista**.
9) Quedan artista y médico para A y C. Artista es hermano de C ⇒ **A = artista** ⇒ **C = médico**.

### Respuestas
1. ¿Quién es el Artista? **A**
2. ¿Quién es el Deportista? **B**
3. ¿Quién es el Médico? **C**

> Extra: D = periodista, E = juez.

### Pregunta 4 (nota)
La pregunta 4 dice: “¿Cuál grupo incluye a una persona que prefiere té pero que no es el juez?”
- Con la deducción anterior, los que prefieren té y NO son juez: **A y C**.
- Eso hace que **A-C-E** y **B-C-E** cumplan la condición.

**Si el examen exige una sola opción**, es posible que el enunciado haya pretendido otra lectura (por ejemplo, que el juez sea C). En ese caso, el único con té que no es juez sería A, y la opción correcta sería **A-C-E**.
