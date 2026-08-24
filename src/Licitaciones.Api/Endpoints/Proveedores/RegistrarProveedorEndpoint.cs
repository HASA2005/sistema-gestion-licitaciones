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
            .Produces<RegistrarProveedorResponse>(StatusCodes.Status201Created);

        return endpoints;
    }

    private static async Task<IResult> EjecutarAsync(
        RegistrarProveedorRequest solicitud,
        RegistrarProveedorService servicio,
        CancellationToken cancellationToken)
    {
        var resultado = await servicio.EjecutarAsync(
            solicitud.Nombre ?? string.Empty,
            cancellationToken);

        return TypedResults.Created(
            uri: (string?)null,
            value: new RegistrarProveedorResponse(resultado.Mensaje));
    }
}
