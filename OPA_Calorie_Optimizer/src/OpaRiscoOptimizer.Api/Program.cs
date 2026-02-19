using System.Text.Json;
using System.Text.Json.Serialization;
using OpaRiscoOptimizer.Api.Services;

/*
 * Punto de entrada de la aplicación (equivalente al Main en versiones antiguas).
 *
 * En .NET moderno (6+), el arranque se hace con "minimal hosting":
 * 1) Se crea el builder (configuración + DI)
 * 2) Se construye la app
 * 3) Se configura el pipeline HTTP (middlewares)
 * 4) Se mapean endpoints/controllers
 * 5) Se arranca el servidor con app.Run()
 */

var builder = WebApplication.CreateBuilder(args);

/*
 * 1) Servicios / Inyección de dependencias (DI)
 * Aquí registramos:
 * - Controllers (API REST)
 * - JSON options (camelCase para el frontend)
 * - Swagger (documentación/ejecución rápida)
 * - Nuestro servicio de negocio: IServicioOptimizador -> ServicioOptimizador
 */
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        // El frontend envía/recibe JSON en camelCase: minCalories, maxWeightKg, items, etc.
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        // No incluir nulls para respuestas más limpias.
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        // Permite enums como string, si se agregan a futuro.
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

        // Estricto con números (evita sorpresas con strings no numéricos).
        o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.Strict;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Lógica de negocio (validaciones + algoritmo).
builder.Services.AddSingleton<IServicioOptimizador, ServicioOptimizador>();

var app = builder.Build();

/*
 * 2) Pipeline HTTP (middlewares)
 * - Swagger solo en Development.
 * - HTTPS redirection.
 * - Archivos estáticos para servir el frontend (wwwroot/index.html, app.js, etc.)
 */
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

/*
 * 3) Frontend
 * UseDefaultFiles hace que al entrar a "/" automáticamente busque:
 * - index.html (por defecto) dentro de wwwroot.
 * UseStaticFiles habilita servir archivos dentro de wwwroot.
 */
app.UseDefaultFiles();
app.UseStaticFiles();

/*
 * 4) Endpoints
 * - Controllers: /api/ejemplo, /api/optimizar
 * - Health: /api/health
 */
app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

/*
 * 5) Arranque del servidor.
 */
app.Run();
