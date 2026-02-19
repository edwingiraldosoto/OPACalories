using System.Text.Json.Serialization;

namespace OpaRiscoOptimizer.Api.Models;

/// <summary>
/// Response (salida) del endpoint POST /api/optimize.
/// </summary>
public sealed class RespuestaOptimizacion
{
    /// <summary>
    /// Indica si se encontró una solución factible (cumple calorías mínimas y peso máximo).
    /// </summary>
    [JsonPropertyName("esFactible")]
    public bool EsFactible { get; set; }

    /// <summary>
    /// Peso total de los items seleccionados (kg).
    /// </summary>
    [JsonPropertyName("pesoTotalKg")]
    public decimal PesoTotalKg { get; set; }

    /// <summary>
    /// Calorías totales de los items seleccionados.
    /// </summary>
    [JsonPropertyName("caloriasTotales")]
    public int CaloriasTotales { get; set; }

    /// <summary>
    /// Lista de items seleccionados por el algoritmo.
    /// </summary>
    [JsonPropertyName("elementosSeleccionados")]
    public List<ElementoSeleccionadoOptimizacion> ElementosSeleccionados { get; set; } = new();

    /// <summary>
    /// Texto descriptivo del algoritmo/estrategia usada (DP por peso, DP por calorías, etc.).
    /// </summary>
    public string Algoritmo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del escalado aplicado para trabajar con enteros en DP (ej: x1000).
    /// </summary>
    [JsonPropertyName("escaladoPeso")]
    public string EscaladoPeso { get; set; } = string.Empty;

    /// <summary>
    /// Notas informativas:
    /// - Mensajes de validación
    /// - Advertencias (por ejemplo, si se usó greedy)
    /// - Comentarios del resultado
    /// </summary>
    [JsonPropertyName("notas")]
    public List<string> Notas { get; set; } = new();
}
