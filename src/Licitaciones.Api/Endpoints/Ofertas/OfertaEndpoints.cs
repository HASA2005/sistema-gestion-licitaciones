using Licitaciones.Application.Ofertas;

namespace Licitaciones.Api.Endpoints.Ofertas;

public sealed record OfertaRequest(Guid LicitacionId, Guid ProveedorId, decimal MontoCrc);
public sealed record OfertaResponse(Guid Id, Guid LicitacionId, Guid ProveedorId, decimal MontoCrc, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public static class OfertaEndpoints
{
    public static IEndpointRouteBuilder MapOfertas(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/v1/ofertas");
        g.MapPost("", async (OfertaRequest r, OfertaService s, CancellationToken ct) => { var oferta = await s.CrearAsync(r.LicitacionId, r.ProveedorId, r.MontoCrc, ct); return Results.Created($"/api/v1/ofertas/{oferta.Id}", oferta); });
        g.MapGet("", async (Guid? licitacionId, Guid? proveedorId, OfertaService s, CancellationToken ct) => Results.Ok(await s.ListarAsync(licitacionId, proveedorId, ct)));
        g.MapGet("/{id:guid}", async (Guid id, OfertaService s, CancellationToken ct) => Results.Ok(await s.ObtenerAsync(id, ct)));
        g.MapPut("/{id:guid}", async (Guid id, OfertaRequest r, OfertaService s, CancellationToken ct) => Results.Ok(await s.EditarAsync(id, r.MontoCrc, ct)));
        g.MapDelete("/{id:guid}", async (Guid id, OfertaService s, CancellationToken ct) => { await s.EliminarAsync(id, ct); return Results.NoContent(); });
        g.MapGet("/licitacion/{licitacionId:guid}/mejor", async (Guid licitacionId, OfertaService s, CancellationToken ct) => Results.Ok(await s.MejorAsync(licitacionId, ct)));
        return app;
    }
}
