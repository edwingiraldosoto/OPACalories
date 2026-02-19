using OpaRiscoOptimizer.Api.Models;
using System.Numerics;

namespace OpaRiscoOptimizer.Api.Services;

/// <summary>
/// Contrato de la lógica de negocio.
/// Se separa del Controller para:
/// - Facilitar pruebas
/// - Mantener el Controller delgado (solo HTTP)
/// </summary>
public interface IServicioOptimizador
{
    /// <summary>
    /// Ejecuta la optimización (0/1 knapsack o Mochila ) para encontrar el subconjunto
    /// 0 (No lo llevas): Dejas el elemento fuera.
    //  1 (Lo llevas): Te llevas el elemento completo.
    /// que cumpla las calorías mínimas sin exceder el peso máximo, minimizando el peso total.
    /// </summary>
    /// <param name="request">
    /// Datos de entrada (vienen del body JSON del request):
    /// - MinCalorias
    /// - PesoMaximoKg
    /// - Elementos
    /// </param>
    /// <returns>Respuesta con solución (o notas de por qué no fue posible).</returns>
    RespuestaOptimizacion Optimizar(SolicitudOptimizacion request);

    /// <summary>
    /// Devuelve un ejemplo listo para probar, basado en el PDF.
    /// </summary>
    SolicitudOptimizacion Ejemplo();
}
