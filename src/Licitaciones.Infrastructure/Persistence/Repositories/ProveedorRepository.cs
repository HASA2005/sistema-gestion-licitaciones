using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

public sealed class ProveedorRepository(
    LicitacionesDbContext contexto) : IProveedorRepository
{
    public async Task<IReadOnlyList<Proveedor>> ListarAsync(CancellationToken cancellationToken = default) => await contexto.Proveedores.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
    public async Task EliminarAsync(Proveedor proveedor, CancellationToken cancellationToken = default) { contexto.Proveedores.Remove(proveedor); await contexto.SaveChangesAsync(cancellationToken); }
    public Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Proveedores.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
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

    public async Task GuardarCambiosAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default)
    {
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
        catch (DbUpdateConcurrencyException)
        {
            contexto.Entry(proveedor).State = EntityState.Detached;
            throw new InvalidOperationException(
                "El proveedor fue modificado por otra operación. Recargue la página e inténtelo nuevamente.");
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
