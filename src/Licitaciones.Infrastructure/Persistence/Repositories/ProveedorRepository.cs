using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excepcion)
            when (EsNombreNormalizadoDuplicado(excepcion))
        {
            contexto.Entry(proveedor).State = EntityState.Detached;
            throw new ProveedorDuplicadoException();
        }
    }

    private static bool EsNombreNormalizadoDuplicado(
        DbUpdateException excepcion)
    {
        return excepcion.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: ProveedorConfiguration.IndiceNombreNormalizado
        };
    }
}
