# OPA Risco Optimizer (API + Frontend) — .NET 8

Prueba técnica: selección de elementos para “reducir el risco” (0/1 Knapsack).

Incluye:
- API REST en C# (.NET 8)
- Frontend liviano (HTML + JS) servido desde `wwwroot/`
- Swagger para pruebas rápidas
- Código comentado y documentado en español

---

## 1) Requisitos

- .NET SDK 8.x
- Visual Studio 2022 / Visual Studio Code

---

## 2) Cómo ejecutar (rápido)

En la carpeta raíz del proyecto:

```bash
cd src/OpaRiscoOptimizer.Api
dotnet run
```

Luego abre en el navegador:

- Frontend: `https://localhost:xxxx/` (raíz)
- Swagger: `https://localhost:xxxx/swagger`
- Health: `https://localhost:xxxx/api/health`

> El puerto cambia según tu máquina (Visual Studio / dotnet lo asigna). Revisa la consola.

---

## 3) Cómo ejecutar y depurar en Visual Studio (recomendado)

1) Abre **Visual Studio 2022**
2) **Archivo > Abrir > Proyecto/Solución**
3) Selecciona el archivo **`OPA_RiscoOptimizer_PRO.sln`**
4) En el *Explorador de Soluciones* define como proyecto de inicio: **OpaRiscoOptimizer.Api**
5) Presiona **F5** (o el botón ▶)

### ¿Por dónde “inicia” el proyecto?

- El arranque está en: `src/OpaRiscoOptimizer.Api/Program.cs`
- Ese archivo:
  - configura Inyección de Dependencias (DI)
  - configura Swagger
  - habilita `wwwroot` (frontend)
  - mapea controladores `/api/...`

### ¿Dónde poner breakpoints?

**Frontend (botón Optimizar):**
- `src/OpaRiscoOptimizer.Api/wwwroot/app.js`
  - función `optimizar()` (se dispara al click del botón `btnSolve`)

**Backend (API):**
- `src/OpaRiscoOptimizer.Api/Controllers/ControladorOptimizacion.cs`
  - método `Optimizar()`
- `src/OpaRiscoOptimizer.Api/Services/ServicioOptimizador.cs`
  - método `Optimizar()`
- `src/OpaRiscoOptimizer.Api/Services/SolucionadorMochila.cs`
  - método `ResolverPorPeso()` o `ResolverPorCalorias()`

---

## 4) API

### GET /api/ejemplo

Devuelve el ejemplo del PDF.

### POST /api/optimizar

**Request (Ejemplo)**
```json
{
  "minCalorias": 15,
  "pesoMaximoKg": 10,
  "elementos": [
    {"id":"E1","nombre":"Elemento 1","pesoKg":5,"calorias":3},
    {"id":"E2","nombre":"Elemento 2","weightKg":3,"calories":5},
    {"id":"E3","nombre":"Elemento 3","pesoKg":5,"calorias":2},
    {"id":"E4","nombre":"Elemento 4","pesoKg":1,"calorias":8},
    {"id":"E5","nombre":"Elemento 5","pesoKg":2,"calorias":3}
  ]
}
```

**Response (Ejemplo)**
```json
{
  "esFactible": true,
  "pesoTotalKg": 6,
  "caloriasTotales": 16,
  "elementosSeleccionados": [
    {"id":"E2","nombre":"Elemento 2","pesoKg":3,"calorias":5},
    {"id":"E4","nombre":"Elemento 4","pesoKg":1,"calorias":8},
    {"id":"E5","nombre":"Elemento 5","pesoKg":2,"calorias":3}
  ],
  "algoritmo": "DP por peso (maximiza calorías y luego elige el menor peso factible)",
  "escaladoPeso": "1kg = 1000g (scale=1000)",
  "notas": []
}
```

---

## 5) Notas del algoritmo

- El problema es un **0/1 Knapsack**.
- La solución principal usa **Programación Dinámica**.
- Para soportar decimales en `pesoMaximoKg`, se escala el peso a entero (gramos): **1 kg = 1000 g**.

Más detalle:
- `docs/approach.md` (Enfoque técnico)
- `docs/scalability.md` (Escalabilidad)

---

## 6) Razonamiento lógico (parte 1)

Respuestas a las preguntas de lógica y explicación:
- `docs/logic_answers.md`

---

## 7) Estructura del Proyecto

```
/OPA_RiscoOptimizer_PRO
  /src/OpaRiscoOptimizer.Api   - Backend .NET 8 y Frontend (wwwroot)
  /docs                        - Documentación adicional y respuestas
  README.md                    - Este archivo
```
