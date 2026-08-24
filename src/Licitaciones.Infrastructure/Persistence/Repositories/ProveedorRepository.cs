using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

public sealed class ProveedorRepository(
    LicitacionesDbContext contexto) : IProveedorRepository
{
    public Task<bool> ExisteConNombreNormalizadoAsync(
        string nombreNormalizado,
        CancellationToken cancellationToken = default)
    {
        return contexto.Proveedores
            .AsNoTracking()
            .AnyAsync(
                proveedor => proveedor.NombreNormalizado == nombreNormalizado,
                cancellationToken);
    }

    public async Task AgregarAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default)
    {
        await contexto.Proveedores.AddAsync(proveedor, cancellationToken);
        await contexto.SaveChangesAsync(cancellationToken);
    }
}
