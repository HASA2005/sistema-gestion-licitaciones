using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.Aprobaciones;
using Licitaciones.Application.TiposCambio;

namespace Licitaciones.Api.Errors;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var clasificacion = Clasificar(exception);

        if (clasificacion.Estado == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Error no controlado. CorrelationId: {CorrelationId}",
                httpContext.TraceIdentifier);
        }

        var problema = new ProblemDetails
        {
            Title = clasificacion.Titulo,
            Status = clasificacion.Estado,
            Detail = clasificacion.Detalle
        };
        problema.Extensions["errorCode"] = clasificacion.Codigo;
        problema.Extensions["correlationId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = clasificacion.Estado;
        await httpContext.Response.WriteAsJsonAsync(
            problema,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }

    private static Clasificacion Clasificar(Exception exception) => exception switch
    {
        BadHttpRequestException => new(
            StatusCodes.Status400BadRequest,
            "Solicitud JSON inválida.",
            "El cuerpo de la solicitud no contiene un JSON válido.",
            "solicitud_json_invalida"),
        LicitacionNoEncontradaException or OfertaNoEncontradaException or KeyNotFoundException => new(
            StatusCodes.Status404NotFound,
            "Recurso no encontrado.",
            exception.Message,
            "recurso_no_encontrado"),
        OfertaDuplicadaException or ProveedorDuplicadoException or LicitacionDuplicadaException or
        LicitacionConcurrenciaException or TipoCambioActivoException => new(
            StatusCodes.Status409Conflict,
            "Conflicto de negocio.",
            exception.Message,
            "conflicto_negocio"),
        ArgumentException or OfertaReglaException or RangoAprobacionTraslapadoException or
        LicitacionConOfertasException or ProveedorConOfertasException => new(
            StatusCodes.Status400BadRequest,
            "Regla de negocio inválida.",
            exception.Message,
            "regla_negocio_invalida"),
        _ => new(
            StatusCodes.Status500InternalServerError,
            "Error interno del servidor.",
            "Ocurrió un error inesperado al procesar la solicitud.",
            "error_interno")
    };

    private sealed record Clasificacion(
        int Estado,
        string Titulo,
        string Detalle,
        string Codigo);
}
