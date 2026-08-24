using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Api.Endpoints.Licitaciones;

/// <summary>
/// Configura el endpoint HTTP para publicar una licitación.
/// </summary>
public static class PublicarLicitacionEndpoint
{
    /// <summary>
    /// Registra la operación <c>POST /api/v1/licitaciones/{id}/publicar</c>.
    /// </summary>
    /// <param name="endpoints">Constructor de rutas de la aplicación.</param>
    /// <returns>El mismo constructor para permitir configuración encadenada.</returns>
    public static IEndpointRouteBuilder MapPublicarLicitacion(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/licitaciones/{id}/publicar",
                EjecutarAsync)
            .WithName("PublicarLicitacion")
            .WithTags("Licitaciones")
            .Produces<PublicarLicitacionResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> EjecutarAsync(
        string id,
        PublicarLicitacionService servicio,
        HttpContext contextoHttp,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var licitacionId))
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status400BadRequest,
                "Identificador de licitación inválido.",
                "El identificador debe ser un UUID válido.",
                "identificador_licitacion_invalido");
        }

        try
        {
            var resultado = await servicio.EjecutarAsync(
                licitacionId,
                cancellationToken);

            return TypedResults.Ok(new PublicarLicitacionResponse(
                resultado.Id,
                resultado.Codigo,
                resultado.Titulo,
                resultado.PresupuestoEstimadoCrc,
                resultado.FechaCierre,
                resultado.Estado.ToString(),
                resultado.UpdatedAt,
                resultado.Mensaje));
        }
        catch (LicitacionNoEncontradaException excepcion)
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status404NotFound,
                "Licitación no encontrada.",
                excepcion.Message,
                "licitacion_no_encontrada");
        }
        catch (PublicacionLicitacionInvalidaException excepcion)
            when (excepcion.Motivo == MotivoPublicacionInvalida.Estado)
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status409Conflict,
                "Estado de licitación no publicable.",
                excepcion.Message,
                "licitacion_estado_no_publicable");
        }
        catch (LicitacionConcurrenciaException excepcion)
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status409Conflict,
                "Conflicto al publicar la licitación.",
                excepcion.Message,
                "licitacion_conflicto_concurrencia");
        }
        catch (PublicacionLicitacionInvalidaException excepcion)
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status422UnprocessableEntity,
                "Datos de licitación no publicables.",
                excepcion.Message,
                "licitacion_datos_no_publicables");
        }
    }

    private static IResult CrearProblema(
        HttpContext contextoHttp,
        int estado,
        string titulo,
        string detalle,
        string codigoError)
    {
        return TypedResults.Problem(
            title: titulo,
            detail: detalle,
            statusCode: estado,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = codigoError,
                ["correlationId"] = contextoHttp.TraceIdentifier
            });
    }
}
