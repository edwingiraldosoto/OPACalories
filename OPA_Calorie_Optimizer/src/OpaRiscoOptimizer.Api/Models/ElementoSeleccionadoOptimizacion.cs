using System.Text.Json.Serialization;

namespace OpaRiscoOptimizer.Api.Models;

/// <summary>
/// Item de salida: corresponde a un elemento seleccionado por el algoritmo.
/// </summary>
public sealed class ElementoSeleccionadoOptimizacion
{
    /// <summary>Id del elemento (E1, E2, ...).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Nombre del elemento.</summary>
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Peso del elemento (kg).</summary>
    [JsonPropertyName("pesoKg")]
    public decimal PesoKg { get; set; }

    /// <summary>Calorías del elemento.</summary>
    [JsonPropertyName("calorias")]
    public int Calorias { get; set; }
}
