using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Licitaciones.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementa la persistencia de licitaciones mediante Entity Framework Core y PostgreSQL.
/// </summary>
/// <param name="contexto">Contexto de datos utilizado por el repositorio.</param>
public sealed class LicitacionRepository(
    LicitacionesDbContext contexto) : ILicitacionRepository
{
    /// <inheritdoc />
    public Task<bool> ExisteConCodigoNormalizadoAsync(
        string codigoNormalizado,
        CancellationToken cancellationToken = default)
    {
        return contexto.Licitaciones
            .AsNoTracking()
            .AnyAsync(
                licitacion =>
                    licitacion.CodigoNormalizado == codigoNormalizado,
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task AgregarAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default)
    {
        await contexto.Licitaciones.AddAsync(
            licitacion,
            cancellationToken);

        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excepcion)
            when (EsCodigoNormalizadoDuplicado(excepcion))
        {
            contexto.Entry(licitacion).State = EntityState.Detached;
            throw new LicitacionDuplicadaException();
        }
    }

    /// <inheritdoc />
    public Task<Licitacion?> ObtenerPorIdAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default)
    {
        return contexto.Licitaciones.SingleOrDefaultAsync(
            licitacion => licitacion.Id == licitacionId,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task GuardarCambiosAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            contexto.Entry(licitacion).State = EntityState.Detached;
            throw new LicitacionConcurrenciaException();
        }
    }

    private static bool EsCodigoNormalizadoDuplicado(
        DbUpdateException excepcion)
    {
        return excepcion.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: LicitacionConfiguration.IndiceCodigoNormalizado
        };
    }
}
