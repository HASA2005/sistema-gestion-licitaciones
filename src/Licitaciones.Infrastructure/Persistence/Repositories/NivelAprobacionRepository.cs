using Licitaciones.Application.Aprobaciones;
using Licitaciones.Domain.Aprobaciones;
using Microsoft.EntityFrameworkCore;
namespace Licitaciones.Infrastructure.Persistence.Repositories;
public sealed class NivelAprobacionRepository(LicitacionesDbContext db) : INivelAprobacionRepository
{ public async Task AgregarAsync(NivelAprobacion n, CancellationToken ct = default) { db.NivelesAprobacion.Add(n); await db.SaveChangesAsync(ct); } public Task<NivelAprobacion?> ObtenerAsync(Guid id, CancellationToken ct = default) => db.NivelesAprobacion.SingleOrDefaultAsync(x => x.Id == id, ct); public async Task<IReadOnlyList<NivelAprobacion>> ListarAsync(CancellationToken ct = default) => await db.NivelesAprobacion.AsNoTracking().OrderBy(x => x.MontoMinimoCrc).ToListAsync(ct); public Task GuardarAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct); public async Task EliminarAsync(NivelAprobacion n, CancellationToken ct = default) { db.Remove(n); await db.SaveChangesAsync(ct); } }
