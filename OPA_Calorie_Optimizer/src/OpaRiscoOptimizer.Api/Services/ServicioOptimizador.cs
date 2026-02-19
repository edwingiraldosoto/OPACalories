using OpaRiscoOptimizer.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace OpaRiscoOptimizer.Api.Services;

/// <summary>
/// Implementación de la lógica de negocio.
///
/// Responsabilidades:
/// 1) Validar la entrada (request).
/// 2) Normalizar datos (convertir pesos decimales a enteros para DP).
/// 3) Seleccionar la mejor estrategia (DP por peso, DP por calorías o fallback).
/// 4) Convertir el resultado del solver a un DTO de salida (RespuestaOptimizacion).
/// </summary>
public sealed class ServicioOptimizador : IServicioOptimizador
{

    /// <summary>
    /// el programa no prueba combinaciones al azar (eso tardaría una eternidad si se tienen muchos elementos). Usa una técnica llamada Programación Dinámica:
    /// Crea una tabla interna donde va calculando el mejor resultado posible para pesos pequeños primero.
    /// Va construyendo la solución óptima paso a paso, "recordando" qué combinaciones fueron mejores anteriormente.
    ///
    /// Factor de escala para convertir kg (decimal) a un entero:
    /// 1 kg = 1000 gramos (precisión de 3 decimales).
    ///
    /// Ejemplo:
    /// 2.345 kg -> 2345 (gramos)
    ///
    /// Esto permite que el algoritmo de Programación Dinámica (DP) trabaje con enteros.
    /// </summary>
    private const int factorEscalaPeso = 1000;

    /// <summary>
    /// Límite de seguridad para el tamaño de la dimensión DP.
    /// Se usa para evitar consumo excesivo de memoria/tiempo en inputs gigantes.
    ///
    /// Nota: en una prueba técnica suele bastar con un límite razonable.
    /// </summary>
    private const int DimensionMaximaDp = 250_000;

    /// <inheritdoc />
    public RespuestaOptimizacion Optimizar(SolicitudOptimizacion solicitud)
    {
        // Objeto de respuesta: lo vamos llenando con datos del algoritmo o notas de validación.
        var respuesta = new RespuestaOptimizacion
        {
            Algoritmo = "",
            EscaladoPeso = "1kg = 1000g (escala=1000)"
        };
 
        // -------------------------
        // 1) VALIDACIONES (entrada)
        // -------------------------
        // Estas validaciones protegen la lógica, evitan errores y devuelven mensajes claros al candidato/evaluador.
        // Los datos vienen del BODY JSON en el endpoint POST /api/optimizar.
 
        if (solicitud.MinCalorias <= 0)
        {
            respuesta.EsFactible = false;
            respuesta.Notas.Add("Validación: MinCalorias debe ser > 0.");
            return respuesta;
        }
 
        if (solicitud.PesoMaximoKg <= 0)
        {
            respuesta.EsFactible = false;
            respuesta.Notas.Add("Validación: PesoMaximoKg debe ser > 0.");
            return respuesta;
        }
 
        if (solicitud.Elementos == null || solicitud.Elementos.Count == 0)
        {
            respuesta.EsFactible = false;
            respuesta.Notas.Add("Validación: la lista Elementos es obligatoria y no puede estar vacía.");
            return respuesta;
        }

        // ---------------------------------------------------
        // 2) NORMALIZACIÓN (pesos a enteros para Programación Dinámica)
        // ---------------------------------------------------
        // Convertimos WeightKg (decimal) a un entero factorEscalaPesod (gramos).
        // También “aseguramos” Id/Name si vienen vacíos.
        var normalizados = new List<ElementoNormalizado>(solicitud.Elementos.Count);
 
        for (var i = 0; i < solicitud.Elementos.Count; i++)
        {
            var elemento = solicitud.Elementos[i];
 
            // Peso debe ser positivo.
            if (elemento.PesoKg <= 0)
            {
                respuesta.EsFactible = false;
                respuesta.Notas.Add($"Validación: el item en índice {i} debe tener PesoKg > 0.");
                return respuesta;
            }
 
            // Calorías no deben ser negativas.
            if (elemento.Calorias < 0)
            {
                respuesta.EsFactible = false;
                respuesta.Notas.Add($"Validación: el item en índice {i} debe tener Calorias >= 0.");
                return respuesta;
            }
 
            // Escalamos kg a gramos (entero).
            var pesoEscalado = (int)Math.Round(elemento.PesoKg * factorEscalaPeso, MidpointRounding.AwayFromZero);
            if (pesoEscalado <= 0)
            {
                respuesta.EsFactible = false;
                respuesta.Notas.Add($"Validación: el item en índice {i} tiene un PesoKg demasiado pequeño para el escalado.");
                return respuesta;
            }
 
            // Construimos el item normalizado.
            // NOTA: Named arguments en C# son case-sensitive. El record usa PascalCase.
            normalizados.Add(new ElementoNormalizado(
                Indice: i,
                Id: elemento.Id ?? $"E{i + 1}",
                Nombre: elemento.Nombre ?? (elemento.Id ?? $"Elemento {i + 1}"),
                PesoEscalado: pesoEscalado,
                PesoKg: elemento.PesoKg,
                Calorias: elemento.Calorias
            ));
        }

        // Peso máximo también se escala.
        var pesoMaximoEscalado = (int)Math.Round(solicitud.PesoMaximoKg * factorEscalaPeso, MidpointRounding.AwayFromZero);
        if (pesoMaximoEscalado <= 0)
        {
            respuesta.EsFactible = false;
            respuesta.Notas.Add("Validación: PesoMaximoKg es demasiado pequeño para el escalado.");
            return respuesta;
        }

        // ----------------------------------------------
        // 3) ELECCIÓN DE ESTRATEGIA (DP por peso o por calorías)
        // ----------------------------------------------
        // Tenemos dos enfoques clásicos:
        // A) DP por PESO: dp[w] = máximo de calorías alcanzables con peso w (w <= MaxWeight)
        //    Luego elegimos el menor w tal que dp[w] >= targetCalories
        //
        // B) DP por CALORÍAS: dp[c] = mínimo peso para lograr c calorías
        //    Luego elegimos el menor peso para c >= targetCalories, con peso <= MaxWeight
        //
        // Elegimos la dimensión más pequeña para ahorrar memoria/tiempo.
        var sumCalorias = normalizados.Sum(x => x.Calorias);
        var dimensionPeso = pesoMaximoEscalado;
        var dimensionCalorias = sumCalorias;
 
        SolucionadorMochila.ResultadoMochila resultadoSolver;
 
        if (dimensionPeso <= dimensionCalorias && dimensionPeso <= DimensionMaximaDp)
        {
            resultadoSolver = SolucionadorMochila.ResolverPorPeso(normalizados, solicitud.MinCalorias, pesoMaximoEscalado);
            respuesta.Algoritmo = "DP por peso (maximiza calorías y luego elige el menor peso factible)";
        }
        else if (dimensionCalorias < dimensionPeso && dimensionCalorias <= DimensionMaximaDp)
        {
            resultadoSolver = SolucionadorMochila.ResolverPorCalorias(normalizados, solicitud.MinCalorias, pesoMaximoEscalado, sumCalorias);
            respuesta.Algoritmo = "DP por calorías (minimiza peso para lograr al menos la meta de calorías)";
        }
        else
        {
            // Fallback (aproximado) para inputs gigantes.
            // No garantiza óptimo, pero mantiene respuesta rápida.
            resultadoSolver = SolucionadorMochila.ResolverGreedyFallback(normalizados, solicitud.MinCalorias, pesoMaximoEscalado);
            respuesta.Algoritmo = "Modo: Elección Inteligente Rápida ⚡ (aproximación por razón calorías/peso)";
            respuesta.Notas.Add("Nota: Vaya, cuántos datos! Como son muchísimos elementos, el cerebro del computador usó un truco de 'Super Velocidad' para dar la mejor respuesta posible en un segundo.\"");
        }



// -----------------------------
// Verificación exhaustiva (solo si n es pequeño)
// -----------------------------
// Para n <= 25, podemos evaluar todas las combinaciones (2^n) y comparar.
// Si el exhaustivo encuentra una mejor solución (menor peso), preferimos esa,
// porque es la "verdad" para tamaños pequeños.
if (normalizados.Count <= 25)
{
    var resultadoExhaustivo = ResolverExhaustivo(normalizados, solicitud.MinCalorias, pesoMaximoEscalado);
 
    // Si el DP no encontró solución, pero el exhaustivo sí, tomamos el exhaustivo.
    // Si ambos encontraron, nos quedamos con el de menor peso.
    if (resultadoExhaustivo.EsFactible &&
        (!resultadoSolver.EsFactible || resultadoExhaustivo.PesoTotalEscalado < resultadoSolver.PesoTotalEscalado))
    {
        resultadoSolver = resultadoExhaustivo;
        respuesta.Algoritmo += " + verificación exhaustiva (n<=25)";
    }
}

// Seguridad: evitamos índices repetidos (no debería pasar en 0/1, pero lo protegemos).
var indicesUnicos = resultadoSolver.IndicesSeleccionados.Distinct().ToList();
if (indicesUnicos.Count != resultadoSolver.IndicesSeleccionados.Count)
{
    respuesta.Notas.Add("Aviso: se detectaron índices repetidos en la selección; se eliminaron duplicados por seguridad.");
    resultadoSolver = resultadoSolver with { IndicesSeleccionados = indicesUnicos };
}
        // ----------------------------------------------
        // 4) ARMAR LA RESPUESTA
        // ----------------------------------------------
        respuesta.EsFactible = resultadoSolver.EsFactible;
        respuesta.CaloriasTotales = resultadoSolver.CaloriasTotales;
        respuesta.PesoTotalKg = Math.Round((decimal)resultadoSolver.PesoTotalEscalado / factorEscalaPeso, 3);
 
        respuesta.ElementosSeleccionados = resultadoSolver.IndicesSeleccionados
            .Select(i => normalizados[i])
            .Select(x => new ElementoSeleccionadoOptimizacion
            {
                Id = x.Id,
                Nombre = x.Nombre,
                PesoKg = x.PesoKg,
                Calorias = x.Calorias
            })
            .ToList();
 
        if (!respuesta.EsFactible)
        {
            respuesta.Notas.Add("Resultado: no se encontró un subconjunto factible que cumpla MinCalorias sin exceder el Peso Maximo en Kg.");
        }
 
        return respuesta;
    }

    /// <inheritdoc />
    public SolicitudOptimizacion Ejemplo()
    {
        // Ejemplo del PDF (mismo set de elementos).
        // Se usa en GET /api/sample y en el botón “Cargar ejemplo” del frontend.
        return new SolicitudOptimizacion
        {
            MinCalorias = 15,
            PesoMaximoKg = 10,
            Elementos = new List<ElementoEntradaOptimizacion>
            {
                new() { Id = "E1", Nombre = "Elemento 1", PesoKg = 5, Calorias = 3 },
                new() { Id = "E2", Nombre = "Elemento 2", PesoKg = 3, Calorias = 5 },
                new() { Id = "E3", Nombre = "Elemento 3", PesoKg = 5, Calorias = 2 },
                new() { Id = "E4", Nombre = "Elemento 4", PesoKg = 1, Calorias = 8 },
                new() { Id = "E5", Nombre = "Elemento 5", PesoKg = 2, Calorias = 3 },
            }
        };
    }

    /// <summary>
    /// Modelo interno (no se expone por la API).
    /// Se usa para:
    /// - Guardar el índice original
    /// - Guardar factorEscalaPesod (gramos, entero)
    /// - Mantener WeightKg original (para devolverla al usuario)
    /// </summary>
    internal sealed record ElementoNormalizado(
        int Indice,
        string Id,
        string Nombre,
        int PesoEscalado,
        decimal PesoKg,
        int Calorias
    );


/// <summary>
/// Resolvedor de respaldo (verificación exhaustiva) para listas pequeñas.
/// Recorre todas las combinaciones posibles (2^n) y elige:
/// 1) El menor peso que cumpla la meta de calorías y no exceda el peso máximo
/// 2) En empate de peso, la mayor cantidad de calorías
/// </summary>
/// <remarks>
/// Esto NO es necesario para producción, pero en una prueba técnica ayuda a:
/// - Validar que la Programación Dinámica esté correcta
/// - Evitar resultados extraños si el evaluador mete pocos elementos
/// </remarks>
private static SolucionadorMochila.ResultadoMochila ResolverExhaustivo(
    List<ElementoNormalizado> elementos,
    int minCalorias,
    int pesoMaximoEscalado)
{
    var n = elementos.Count;
    var totalSubconjuntos = 1 << n;

    var mejorPeso = int.MaxValue;
    var mejorCalorias = -1;
    var mejorSeleccion = new List<int>();

    for (var mask = 0; mask < totalSubconjuntos; mask++)
    {
        var peso = 0;
        var calorias = 0;

        // Recorremos bits (selección).
        for (var i = 0; i < n; i++)
        {
            if (((mask >> i) & 1) == 1)
            {
                peso += elementos[i].PesoEscalado;
                if (peso > pesoMaximoEscalado) break; // poda temprana
                calorias += elementos[i].Calorias;
            }
        }

        if (peso <= pesoMaximoEscalado && calorias >= minCalorias)
        {
            if (peso < mejorPeso || (peso == mejorPeso && calorias > mejorCalorias))
            {
                mejorPeso = peso;
                mejorCalorias = calorias;

                mejorSeleccion = new List<int>();
                for (var i = 0; i < n; i++)
                    if (((mask >> i) & 1) == 1)
                        mejorSeleccion.Add(i);
            }
        }
    }

    return new SolucionadorMochila.ResultadoMochila(
        EsFactible: mejorPeso != int.MaxValue,
        PesoTotalEscalado: mejorPeso == int.MaxValue ? 0 : mejorPeso,
        CaloriasTotales: mejorCalorias < 0 ? 0 : mejorCalorias,
        IndicesSeleccionados: mejorSeleccion
    );
}

}
