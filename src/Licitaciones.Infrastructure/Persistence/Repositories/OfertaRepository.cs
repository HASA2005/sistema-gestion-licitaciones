using Licitaciones.Application.Ofertas;
using Licitaciones.Domain.Ofertas;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;
public sealed class OfertaRepository(LicitacionesDbContext db) : IOfertaRepository
{
    public async Task AgregarAsync(Oferta oferta, CancellationToken ct = default)
    {
        await db.Ofertas.AddAsync(oferta, ct);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "ux_ofertas_licitacion_proveedor" }) { throw new OfertaDuplicadaException(); }
    }
    public Task<Oferta?> ObtenerAsync(Guid id, CancellationToken ct = default) => db.Ofertas.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<Oferta>> ListarAsync(Guid? l, Guid? p, CancellationToken ct = default) => await db.Ofertas.AsNoTracking().Where(x => !l.HasValue || x.LicitacionId == l).Where(x => !p.HasValue || x.ProveedorId == p).OrderBy(x => x.CreatedAt).ToListAsync(ct);
    public Task<bool> ExisteParaParejaAsync(Guid l, Guid p, Guid? excluirId = null, CancellationToken ct = default) => db.Ofertas.AnyAsync(x => x.LicitacionId == l && x.ProveedorId == p && (!excluirId.HasValue || x.Id != excluirId), ct);
    public Task GuardarAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
    public async Task EliminarAsync(Oferta oferta, CancellationToken ct = default) { db.Ofertas.Remove(oferta); await db.SaveChangesAsync(ct); }
}
