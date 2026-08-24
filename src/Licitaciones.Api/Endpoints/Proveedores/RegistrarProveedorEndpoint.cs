using Licitaciones.Application.Proveedores;

namespace Licitaciones.Api.Endpoints.Proveedores;

public static class RegistrarProveedorEndpoint
{
    public static IEndpointRouteBuilder MapRegistrarProveedor(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                "/api/v1/proveedores",
                EjecutarAsync)
            .WithName("RegistrarProveedor")
            .WithTags("Proveedores")
            .Produces<RegistrarProveedorResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static async Task<IResult> EjecutarAsync(
        RegistrarProveedorRequest solicitud,
        RegistrarProveedorService servicio,
        HttpContext contextoHttp,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await servicio.EjecutarAsync(
                solicitud.Nombre ?? string.Empty,
                cancellationToken);

            return TypedResults.Created(
                uri: (string?)null,
                value: new RegistrarProveedorResponse(resultado.Mensaje));
        }
        catch (ProveedorDuplicadoException excepcion)
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status409Conflict,
                "Proveedor duplicado.",
                excepcion.Message,
                "proveedor_duplicado");
        }
        catch (ArgumentException excepcion)
        {
            return CrearProblema(
                contextoHttp,
                StatusCodes.Status422UnprocessableEntity,
                "Datos del proveedor inválidos.",
                excepcion.Message,
                "proveedor_nombre_invalido");
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
