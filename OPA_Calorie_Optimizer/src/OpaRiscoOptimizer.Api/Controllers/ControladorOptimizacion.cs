using Microsoft.AspNetCore.Mvc;
using OpaRiscoOptimizer.Api.Models;
using OpaRiscoOptimizer.Api.Services;

namespace OpaRiscoOptimizer.Api.Controllers;

/// <summary>
/// Controlador HTTP principal de la prueba.
///
/// Este controlador expone endpoints REST que son consumidos por:
/// 1) Swagger (para pruebas manuales)
/// 2) El Frontend (wwwroot/app.js) mediante fetch()
///
/// Rutas disponibles:
/// - GET  /api/sample    -> devuelve un ejemplo (mismo set del PDF)
/// - POST /api/optimize  -> ejecuta la optimización (knapsack 0/1)
/// </summary>
[ApiController]
[Route("api")]
public sealed class ControladorOptimizacion : ControllerBase
{
    private readonly IServicioOptimizador _servicio;

    /// <summary>
    /// Inyección de dependencias del servicio de optimización.
    /// </summary>
    /// <param name="service">
    /// Servicio que contiene la lógica de negocio (validaciones + algoritmo).
    /// </param>
    public ControladorOptimizacion(IServicioOptimizador servicio)
    {
        _servicio = servicio;
    }

    /// <summary>
    /// Devuelve un payload de ejemplo 
    /// Útil para probar rápido desde el frontend o Swagger.
    /// </summary>
    /// <returns>Un <see cref="SolicitudOptimizacion"/> listo para enviar a /api/optimizar.</returns>
    [HttpGet("ejemplo")]
    public ActionResult<SolicitudOptimizacion> Ejemplo()
        => Ok(_servicio.Ejemplo());

    /// <summary>
    /// Calcula el subconjunto óptimo de elementos para escalar el risco.
    ///
    /// Objetivo:
    /// - Cumplir: calorías totales >= MinCalorias
    /// - Cumplir: peso total <= PesoMaximoKg
    /// - Optimizar: minimizar el peso total (entre las soluciones factibles)
    ///
    /// Nota:
    /// Los parámetros llegan en el BODY del request (JSON),
    /// porque el frontend envía un fetch POST con el objeto SolicitudOptimizacion.
    /// </summary>
    /// <param name="request">
    /// Datos de entrada enviados por el usuario (frontend o Swagger):
    /// - MinCalorias: calorías mínimas requeridas.
    /// - PesoMaximoKg: peso máximo permitido (kg).
    /// - Elementos: lista de elementos con peso y calorías.
    /// </param>
    /// <returns>
    /// Respuesta con:
    /// - EsFactible: si encontró solución factible.
    /// - ElementosSeleccionados: lista de elementos seleccionados.
    /// - PesoTotalKg y CaloriasTotales: totales resultantes.
    /// - Notas: notas de validación/algoritmo.
    /// </returns>
    [HttpPost("optimizar")]
    public ActionResult<RespuestaOptimizacion> Optimizar([FromBody] SolicitudOptimizacion request)
        => Ok(_servicio.Optimizar(request));
}
