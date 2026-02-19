# Enfoque de solución (0/1 Knapsack)

## Objetivo
Dado:
- `MinCalorias` (mínimo de calorías)
- `PesoMaximoKg` (peso máximo permitido)
- Lista de elementos con `PesoKg` y `Calorias`

Se busca **un subconjunto** que:
1. Cumpla `TotalCalorias >= MinCalorias`
2. Cumpla `TotalPeso <= PesoMaximoKg`
3. **Minimice** el peso total (entre todas las soluciones viables)

Esto es una variación clásica del problema **0/1 Knapsack**.

## Detalle de implementación

### 1) Escalado de peso
Para soportar decimales en kg, se transforma:

- `pesoEscalado = round(PesoKg * 1000)`

Es decir, trabajamos en **gramos** (enteros).

### 2) Programación dinámica
Se implementan dos DP y se elige automáticamente la mejor según el tamaño:

#### A) DP por peso (recomendada cuando `MaxWeightScaled` es pequeño)
- `dp[w] = máximo de calorías alcanzables con peso exacto w`

Luego se busca el **menor** `w` tal que `dp[w] >= MinCalories`.

Complejidad:
- Tiempo: `O(n * W)`
- Memoria: `O(W)`

#### B) DP por calorías (recomendada cuando la suma de calorías es pequeña)
- `dp[c] = mínimo peso necesario para alcanzar c calorías`

Luego se busca el `c >= MinCalories` con `dp[c] <= MaxWeight` y con menor peso.

Complejidad:
- Tiempo: `O(n * sumCalories)`
- Memoria: `O(sumCalories)`

### 3) Reconstrucción
En ambos enfoques se guardan arreglos `parent` para reconstruir el conjunto seleccionado.

### 4) Fallback (solo si el input es enorme)
Si ambas dimensiones sobrepasan un umbral de seguridad, se usa un greedy por ratio calorías/peso.
Esto **no es óptimo**, pero mantiene la app responsiva.
