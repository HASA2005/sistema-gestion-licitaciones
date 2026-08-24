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
            return TypedResults.Problem(
                title: "Proveedor duplicado.",
                detail: excepcion.Message,
                statusCode: StatusCodes.Status409Conflict,
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = "proveedor_duplicado",
                    ["correlationId"] = contextoHttp.TraceIdentifier
                });
        }
        catch (ArgumentException excepcion)
        {
            return TypedResults.Problem(
                title: "Datos del proveedor inválidos.",
                detail: excepcion.Message,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = "proveedor_nombre_invalido",
                    ["correlationId"] = contextoHttp.TraceIdentifier
                });
        }
    }
}
