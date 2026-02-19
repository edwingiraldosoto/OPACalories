using System.Text.Json.Serialization;

namespace OpaRiscoOptimizer.Api.Models;

/// <summary>
/// Elemento individual de entrada para la optimización.
/// </summary>
public sealed class ElementoEntradaOptimizacion
{
    /// <summary>
    /// Identificador del lado del cliente (por ejemplo: "E1").
    /// Es opcional; si no llega, el backend asigna uno automáticamente.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Nombre o etiqueta visible para el usuario (por ejemplo: "Elemento 1").
    /// Es opcional; si no llega, el backend asigna un nombre basado en Id o el índice.
    /// </summary>
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    /// <summary>
    /// Peso del elemento en kilogramos.
    /// Debe ser > 0 (validación en el servicio).
    /// </summary>
    [JsonPropertyName("pesoKg")]
    public decimal PesoKg { get; set; }

    /// <summary>
    /// Calorías aportadas por el elemento.
    /// Debe ser >= 0 (validación en el servicio).
    /// </summary>
    [JsonPropertyName("calorias")]
    public int Calorias { get; set; }
}
