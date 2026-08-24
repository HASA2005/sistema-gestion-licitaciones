using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;

namespace Licitaciones.Api.Errors;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var esSolicitudInvalida = exception is BadHttpRequestException or ArgumentException or OfertaReglaException;
        var estado = exception switch { OfertaNoEncontradaException => 404, OfertaDuplicadaException or ProveedorDuplicadoException or LicitacionDuplicadaException => 409, _ when esSolicitudInvalida => 400, _ => 500 };

        if (!esSolicitudInvalida)
        {
            logger.LogError(
                exception,
                "Error no controlado. CorrelationId: {CorrelationId}",
                httpContext.TraceIdentifier);
        }

        var problema = new ProblemDetails
        {
            Title = esSolicitudInvalida
                ? "Solicitud JSON inválida."
                : "Error interno del servidor.",
            Status = estado,
            Detail = esSolicitudInvalida
                ? "El cuerpo de la solicitud no contiene un JSON válido."
                : "Ocurrió un error inesperado al procesar la solicitud."
        };
        problema.Extensions["errorCode"] = esSolicitudInvalida
            ? "solicitud_json_invalida"
            : "error_interno";
        problema.Extensions["correlationId"] =
            httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = estado;
        await httpContext.Response.WriteAsJsonAsync(
            problema,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}
