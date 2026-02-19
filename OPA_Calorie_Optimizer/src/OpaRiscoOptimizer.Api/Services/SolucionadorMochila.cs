using System;
using System.Collections.Generic;
using System.Linq;

namespace OpaRiscoOptimizer.Api.Services;

/// <summary>
/// Implementa el corazón del algoritmo: 0/1 Knapsack con dos variantes DP.
///
/// Este archivo no conoce de HTTP ni de modelos de entrada/salida.
/// Solo recibe una lista de items normalizados (pesos enteros) y devuelve un resultado.
/// </summary>
internal static class SolucionadorMochila
{
    /// <summary>
    /// Resultado estándar del solver.
    /// TotalWeightScaled está en "gramos" (escala 1000), NO en kg.
    /// SelectedIndexes son índices dentro de la lista recibida.
    /// </summary>
    internal sealed record ResultadoMochila(
        bool EsFactible,
        int PesoTotalEscalado,
        int CaloriasTotales,
        List<int> IndicesSeleccionados
    );

    /// <summary>
    /// DP por PESO.
    ///
    /// dp[w] = máximas calorías alcanzables usando peso exacto w (0..W).
    /// Luego buscamos el menor w tal que dp[w] >= targetCalories.
    ///
    /// Ventaja: si W es pequeño, es muy eficiente.
    /// </summary>
    /// <param name="items">Lista de items con WeightScaled (entero) y Calories.</param>
    /// <param name="targetCalories">Calorías mínimas requeridas.</param>
    /// <param name="maxWeightScaled">Peso máximo permitido (entero, en gramos).</param>
    public static ResultadoMochila ResolverPorPeso(
        IReadOnlyList<ServicioOptimizador.ElementoNormalizado> items,
        int targetCalories,
        int maxWeightScaled
    )
    {
        var n = items.Count;
        var W = maxWeightScaled;

        // dp[w] = calorías máximas alcanzables con peso w.
        // -1 significa "inaccesible".
        var dp = new int[W + 1];
        Array.Fill(dp, -1);
        dp[0] = 0;

        // Estructuras para reconstrucción:
        // padreW[w] = peso anterior desde el cual llegamos a w
        // padreI[w] = índice del item usado para llegar a w
        var padreW = new int[W + 1];
        var padreI = new int[W + 1];
        Array.Fill(padreW, -1);
        Array.Fill(padreI, -1);

        // Recorremos items y actualizamos dp de atrás hacia adelante (0/1 knapsack).
        for (var i = 0; i < n; i++)
        {
            var wi = items[i].PesoEscalado;
            var ci = items[i].Calorias;
            if (wi > W) continue;

            for (var w = W; w >= wi; w--)
            {
                if (dp[w - wi] == -1) continue;

                var cand = dp[w - wi] + ci;
                if (cand > dp[w])
                {
                    dp[w] = cand;
                    padreW[w] = w - wi;
                    padreI[w] = i;
                }
            }
        }

        // Elegimos el menor peso w que cumpla las calorías mínimas.
        var bestW = -1;
        for (var w = 0; w <= W; w++)
        {
            if (dp[w] >= targetCalories)
            {
                bestW = w;
                break;
            }
        }

        if (bestW == -1)
            return new ResultadoMochila(false, 0, 0, new List<int>());
 
        var seleccionados = ReconstruirPorPeso(padreW, padreI, bestW);
        var caloriasTotales = seleccionados.Sum(i => items[i].Calorias);
 
        return new ResultadoMochila(true, bestW, caloriasTotales, seleccionados);
    }

    /// <summary>
    /// DP por CALORÍAS.
    ///
    /// dpWeight[c] = peso mínimo requerido para lograr exactamente c calorías.
    /// Luego buscamos el mejor c >= targetCalories con peso <= maxWeightScaled
    /// y que tenga el menor peso posible.
    ///
    /// Ventaja: cuando la suma total de calorías es menor que W, esta variante es mejor.
    /// </summary>
    /// <param name="items">Lista de items.</param>
    /// <param name="targetCalories">Meta mínima de calorías.</param>
    /// <param name="maxWeightScaled">Peso máximo permitido (gramos).</param>
    /// <param name="sumCalories">Suma de calorías de todos los items (para tamaño de dp).</param>
    public static ResultadoMochila ResolverPorCalorias(
        IReadOnlyList<ServicioOptimizador.ElementoNormalizado> items,
        int targetCalories,
        int maxWeightScaled,
        int sumCalories
    )
    {
        var n = items.Count;

        // Tamaño máximo de la dimensión calorías.
        // Aseguramos que al menos llegue a targetCalories.
        var C = Math.Max(sumCalories, targetCalories);

        const int INF = int.MaxValue / 4;

        // dpWeight[c] = peso mínimo para alcanzar c calorías.
        var dpWeight = new int[C + 1];
        Array.Fill(dpWeight, INF);
        dpWeight[0] = 0;

        // Para reconstrucción:
        // padreC[c] = calorías anteriores
        // padreI[c] = item usado
        var padreC = new int[C + 1];
        var padreI = new int[C + 1];
        Array.Fill(padreC, -1);
        Array.Fill(padreI, -1);

        for (var i = 0; i < n; i++)
        {
            var wi = items[i].PesoEscalado;
            var ci = items[i].Calorias;

            // Si un item tiene 0 calorías, no aporta a la meta -> lo podemos ignorar en este DP.
            if (ci <= 0) continue;

            for (var c = C; c >= ci; c--)
            {
                if (dpWeight[c - ci] == INF) continue;

                var candW = dpWeight[c - ci] + wi;
                if (candW < dpWeight[c])
                {
                    dpWeight[c] = candW;
                    padreC[c] = c - ci;
                    padreI[c] = i;
                }
            }
        }

        // Buscar c >= targetCalories con peso <= maxWeightScaled y con menor peso.
        var bestC = -1;
        var bestW = INF;

        for (var c = targetCalories; c <= C; c++)
        {
            var w = dpWeight[c];
            if (w <= maxWeightScaled && w < bestW)
            {
                bestW = w;
                bestC = c;
            }
        }

        if (bestC == -1)
            return new ResultadoMochila(false, 0, 0, new List<int>());
 
        var seleccionados = ReconstruirPorCalorias(padreC, padreI, bestC);
        var caloriasTotales = seleccionados.Sum(i => items[i].Calorias);
        var pesoTotal = seleccionados.Sum(i => items[i].PesoEscalado);
 
        return new ResultadoMochila(true, pesoTotal, caloriasTotales, seleccionados);
    }

    /// <summary>
    /// Algoritmo de aproximación rápida para optimizar el rendimiento en listas extensas.
    /// Selecciona elementos basándose en su valor relativo de calorías por cada kilogramo.
    /// </summary>
    public static ResultadoMochila ResolverGreedyFallback(
        IReadOnlyList<ServicioOptimizador.ElementoNormalizado> items,
        int targetCalories,
        int maxWeightScaled
    )
    {
        var ordered = items
            .Select((it, idx) => new
            {
                idx,
                it.PesoEscalado,
                it.Calorias,
                ratio = it.Calorias <= 0 ? 0 : (double)it.Calorias / Math.Max(1, it.PesoEscalado)
            })
            .OrderByDescending(x => x.ratio)
            .ThenBy(x => x.PesoEscalado)
            .ToList();

        var seleccionados = new List<int>();
        var sumaPeso = 0;
        var sumaCalorias = 0;
 
        foreach (var x in ordered)
        {
            if (sumaCalorias >= targetCalories) break;
            if (sumaPeso + x.PesoEscalado > maxWeightScaled) continue;
 
            seleccionados.Add(x.idx);
            sumaPeso += x.PesoEscalado;
            sumaCalorias += x.Calorias;
        }
 
        return new ResultadoMochila(sumaCalorias >= targetCalories, sumaPeso, sumaCalorias, seleccionados);
    }

    /// <summary>
    /// Reconstruye la lista de items seleccionados cuando se usó DP por peso.
    /// </summary>
    private static List<int> ReconstruirPorPeso(int[] padreW, int[] padreI, int bestW)
    {
        var seleccionados = new List<int>();
        var w = bestW;
 
        while (w > 0)
        {
            var i = padreI[w];
            if (i < 0) break;
 
            seleccionados.Add(i);
            w = padreW[w];
            if (w < 0) break;
        }
 
        seleccionados.Reverse();
        return seleccionados;
    }

    /// <summary>
    /// Reconstruye la lista de items seleccionados cuando se usó DP por calorías.
    /// </summary>
    private static List<int> ReconstruirPorCalorias(int[] padreC, int[] padreI, int bestC)
    {
        var seleccionados = new List<int>();
        var c = bestC;

        while (c > 0)
        {
            var i = padreI[c];
            if (i < 0) break;

            seleccionados.Add(i);
            c = padreC[c];
            if (c < 0) break;
        }

        seleccionados.Reverse();
        return seleccionados;
    }
}
