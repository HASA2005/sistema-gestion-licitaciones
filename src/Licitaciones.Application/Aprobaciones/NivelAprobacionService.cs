using Licitaciones.Domain.Aprobaciones;
namespace Licitaciones.Application.Aprobaciones;
public interface INivelAprobacionRepository { Task AgregarAsync(NivelAprobacion n, CancellationToken ct = default); Task<NivelAprobacion?> ObtenerAsync(Guid id, CancellationToken ct = default); Task<IReadOnlyList<NivelAprobacion>> ListarAsync(CancellationToken ct = default); Task GuardarAsync(CancellationToken ct = default); Task EliminarAsync(NivelAprobacion n, CancellationToken ct = default); }
public sealed class NivelAprobacionService(INivelAprobacionRepository repo)
{
    public async Task<NivelAprobacion> CrearAsync(string responsable, decimal min, decimal? max, CancellationToken ct = default) { var n = new NivelAprobacion(responsable, min, max); ValidarTraslape(n, await repo.ListarAsync(ct)); await repo.AgregarAsync(n, ct); return n; }
    public async Task<IReadOnlyList<NivelAprobacion>> ListarAsync(CancellationToken ct = default) => await repo.ListarAsync(ct);
    public async Task<NivelAprobacion> ObtenerAsync(Guid id, CancellationToken ct = default) => await repo.ObtenerAsync(id, ct) ?? throw new KeyNotFoundException("El nivel no existe.");
    public async Task EditarAsync(Guid id, string r, decimal min, decimal? max, CancellationToken ct = default) { var n = await ObtenerAsync(id, ct); var candidato = new NivelAprobacion(r, min, max); ValidarTraslape(candidato, (await repo.ListarAsync(ct)).Where(x => x.Id != id)); n.Editar(r, min, max); await repo.GuardarAsync(ct); }
    public async Task EliminarAsync(Guid id, CancellationToken ct = default) => await repo.EliminarAsync(await ObtenerAsync(id, ct), ct);
    public async Task<NivelAprobacion> DeterminarAsync(decimal monto, CancellationToken ct = default) => (await repo.ListarAsync(ct)).FirstOrDefault(x => x.Incluye(monto)) ?? throw new KeyNotFoundException("No existe un nivel para el monto indicado.");
    private static void ValidarTraslape(NivelAprobacion n, IEnumerable<NivelAprobacion> otros) { foreach (var x in otros) if ((n.MontoMaximoCrc is null || x.MontoMinimoCrc <= n.MontoMaximoCrc) && (x.MontoMaximoCrc is null || n.MontoMinimoCrc <= x.MontoMaximoCrc)) throw new RangoAprobacionTraslapadoException(); }
}
