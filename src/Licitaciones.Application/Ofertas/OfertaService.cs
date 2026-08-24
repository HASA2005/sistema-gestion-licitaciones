using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;

namespace Licitaciones.Application.Ofertas;

public sealed record OfertaDto(Guid Id, Guid LicitacionId, Guid ProveedorId, decimal MontoCrc, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
public sealed record MejorOfertaDto(OfertaDto? Oferta, decimal? AhorroPorcentaje, string Clasificacion);

public sealed class OfertaService(
    IOfertaRepository ofertas, ILicitacionRepository licitaciones, IProveedorRepository proveedores,
    TimeProvider? reloj = null)
{
    private readonly TimeProvider reloj = reloj ?? TimeProvider.System;

    public async Task<OfertaDto> CrearAsync(Guid licitacionId, Guid proveedorId, decimal monto, CancellationToken ct = default)
    {
        var licitacion = await ObtenerLicitacion(licitacionId, ct);
        if (licitacion.Estado != EstadoLicitacion.Publicada) throw new OfertaReglaException("Solo se pueden registrar ofertas para licitaciones publicadas.");
        var ahora = reloj.GetUtcNow();
        if (licitacion.FechaCierre <= ahora) throw new OfertaReglaException("La licitación ya está cerrada o vencida.");
        if (monto > licitacion.PresupuestoEstimadoCrc) throw new OfertaReglaException("El monto ofertado no puede superar el presupuesto de la licitación.");
        if (await proveedores.ObtenerPorIdAsync(proveedorId, ct) is null) throw new OfertaReglaException("El proveedor no existe.");
        if (await ofertas.ExisteParaParejaAsync(licitacionId, proveedorId, null, ct)) throw new OfertaDuplicadaException();
        var oferta = new Oferta(licitacionId, proveedorId, monto, ahora); await ofertas.AgregarAsync(oferta, ct); return Map(oferta);
    }

    public async Task<OfertaDto> ObtenerAsync(Guid id, CancellationToken ct = default) => Map(await ObtenerOferta(id, ct));
    public async Task<IReadOnlyList<OfertaDto>> ListarAsync(Guid? licitacionId, Guid? proveedorId, CancellationToken ct = default) => (await ofertas.ListarAsync(licitacionId, proveedorId, ct)).Select(Map).ToList();

    public async Task<OfertaDto> EditarAsync(Guid id, decimal monto, CancellationToken ct = default)
    {
        var oferta = await ObtenerOferta(id, ct); var licitacion = await ObtenerLicitacion(oferta.LicitacionId, ct); var ahora = reloj.GetUtcNow();
        ValidarAbierta(licitacion, ahora); if (monto > licitacion.PresupuestoEstimadoCrc) throw new OfertaReglaException("El monto ofertado no puede superar el presupuesto de la licitación.");
        oferta.Editar(monto, ahora); await ofertas.GuardarAsync(ct); return Map(oferta);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var oferta = await ObtenerOferta(id, ct); var licitacion = await ObtenerLicitacion(oferta.LicitacionId, ct); ValidarAbierta(licitacion, reloj.GetUtcNow()); await ofertas.EliminarAsync(oferta, ct);
    }

    public async Task<MejorOfertaDto> MejorAsync(Guid licitacionId, CancellationToken ct = default)
    {
        var licitacion = await ObtenerLicitacion(licitacionId, ct); var lista = (await ofertas.ListarAsync(licitacionId, null, ct)).Where(x => x.MontoCrc > 0 && x.MontoCrc <= licitacion.PresupuestoEstimadoCrc).OrderBy(x => x.MontoCrc).ThenBy(x => x.CreatedAt).ToList();
        if (lista.Count == 0) return new(null, null, "Sin ofertas válidas");
        var mejor = lista[0]; var ahorro = (licitacion.PresupuestoEstimadoCrc - mejor.MontoCrc) / licitacion.PresupuestoEstimadoCrc * 100m;
        var clasificacion = ahorro == 0 ? "Oferta válida sin ahorro" : ahorro >= 10 ? "Oferta conveniente" : "Oferta aceptable";
        return new(Map(mejor), ahorro, clasificacion);
    }

    private async Task<Licitacion> ObtenerLicitacion(Guid id, CancellationToken ct) => await licitaciones.ObtenerPorIdAsync(id, ct) ?? throw new OfertaReglaException("La licitación no existe.");
    private async Task<Oferta> ObtenerOferta(Guid id, CancellationToken ct) => await ofertas.ObtenerAsync(id, ct) ?? throw new OfertaNoEncontradaException();
    private static void ValidarAbierta(Licitacion l, DateTimeOffset ahora) { if (l.Estado != EstadoLicitacion.Publicada || l.FechaCierre <= ahora) throw new OfertaReglaException("Las ofertas de licitaciones cerradas o vencidas no se pueden modificar."); }
    private static OfertaDto Map(Oferta x) => new(x.Id, x.LicitacionId, x.ProveedorId, x.MontoCrc, x.CreatedAt, x.UpdatedAt);
}
