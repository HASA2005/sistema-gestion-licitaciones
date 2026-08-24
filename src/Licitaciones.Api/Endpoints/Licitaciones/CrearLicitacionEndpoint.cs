using Licitaciones.Application.Licitaciones;

namespace Licitaciones.Api.Endpoints.Licitaciones;

/// <summary>
/// Configura el endpoint HTTP para crear licitaciones en estado Borrador.
/// </summary>
public static class CrearLicitacionEndpoint
{
    /// <summary>
    /// Registra la operación <c>POST /api/v1/licitaciones</c>.
    /// </summary>
    /// <param name="endpoints">Constructor de rutas de la aplicación.</param>
    /// <returns>El mismo constructor de rutas para permitir configuración encadenada.</returns>
    public static IEndpointRouteBuilder MapCrearLicitacion(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/licitaciones",
                EjecutarAsync)
            .WithName("CrearLicitacion")
            .WithTags("Licitaciones")
            .Produces<CrearLicitacionResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status415UnsupportedMediaType)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> EjecutarAsync(
        CrearLicitacionRequest solicitud,
        CrearLicitacionService servicio,
        HttpContext contextoHttp,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await servicio.EjecutarAsync(
                new CrearLicitacionComando(
                    solicitud.Codigo ?? string.Empty,
                    solicitud.Titulo ?? string.Empty,
                    solicitud.PresupuestoEstimadoCrc ?? 0m,
                    solicitud.FechaCierre ?? default),
                cancellationToken);

            return TypedResults.Created(
                uri: (string?)null,
                value: new CrearLicitacionResponse(
                    resultado.Id,
                    resultado.Codigo,
                    resultado.Titulo,
                    resultado.PresupuestoEstimadoCrc,
                    resultado.FechaCierre,
                    resultado.Estado.ToString(),
                    resultado.Mensaje));
        }
        catch (LicitacionDuplicadaException excepcion)
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status409Conflict,
                "Código de licitación duplicado.",
                excepcion.Message,
                "licitacion_codigo_duplicado");
        }
        catch (ArgumentException excepcion)
        {
            var mensaje = ObtenerMensajeSeguro(excepcion);
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status422UnprocessableEntity,
                "Datos de la licitación inválidos.",
                mensaje,
                "licitacion_datos_invalidos");
        }
    }

    private static string ObtenerMensajeSeguro(ArgumentException excepcion)
    {
        var mensaje = excepcion.Message.Split(Environment.NewLine)[0];
        if (string.IsNullOrWhiteSpace(excepcion.ParamName))
        {
            return mensaje;
        }

        var inicioSufijo = mensaje.LastIndexOf(" (", StringComparison.Ordinal);
        if (inicioSufijo >= 0 && mensaje.IndexOf(
                excepcion.ParamName,
                inicioSufijo,
                StringComparison.Ordinal) >= 0)
        {
            mensaje = mensaje[..inicioSufijo];
        }

        return mensaje;
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
