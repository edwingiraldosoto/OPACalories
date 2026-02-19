# Escalabilidad

La solución usa Programación Dinámica, que es óptima pero depende de la **dimensión** elegida:

- DP por peso: `O(n * MaxWeightScaled)`
- DP por calorías: `O(n * sumCalories)`

## Estrategia usada
En `OptimizerService` se calcula:
- `weightDim = MaxWeightScaled`
- `caloriesDim = sumCalories`

Y se elige la DP de menor dimensión, con un límite de seguridad (`MaxDpDimension`).

## Para producción
Si este problema creciera (miles de ítems con pesos máximos enormes), opciones:

- Heurísticas / aproximaciones (p.ej. *greedy + local search*)
- Branch & bound
- Meet-in-the-middle (si n es moderado)
- Servicios asíncronos + caching
- Validación de entradas y límites por negocio
