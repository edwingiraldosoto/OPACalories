using System.Text.Json.Serialization;

namespace OpaRiscoOptimizer.Api.Models;

/// <summary>
/// Request (entrada) para el endpoint POST /api/optimize.
///
/// Nota sobre el origen de los datos:
/// - El frontend (wwwroot/app.js) construye un JSON con estos campos y lo envía por fetch().
/// - Swagger también permite enviar el mismo JSON.
/// </summary>
public sealed class SolicitudOptimizacion
{
    /// <summary>
    /// Calorías mínimas requeridas para considerar que se "reduce el risco".
    /// Regla del problema: la suma de Calories de los items seleccionados debe ser >= MinCalories.
    /// </summary>
    /// <summary>
    /// Calorías mínimas requeridas.
    /// </summary>
    [JsonPropertyName("minCalorias")]
    public int MinCalorias { get; set; }

    /// <summary>
    /// Peso máximo permitido en kilogramos.
    /// Regla del problema: la suma de WeightKg de los items seleccionados debe ser <= MaxWeightKg.
    /// </summary>
    /// <summary>
    /// Peso máximo permitido en kilogramos.
    /// </summary>
    [JsonPropertyName("pesoMaximoKg")]
    public decimal PesoMaximoKg { get; set; }

    /// <summary>
    /// Lista de elementos disponibles para seleccionar (0/1 knapsack: se toma o no se toma).
    /// </summary>
    /// <summary>
    /// Lista de elementos disponibles.
    /// </summary>
    [JsonPropertyName("elementos")]
    public List<ElementoEntradaOptimizacion> Elementos { get; set; } = new();
}
