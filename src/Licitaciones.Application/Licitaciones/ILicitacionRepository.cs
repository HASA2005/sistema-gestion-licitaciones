using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Define las operaciones de persistencia requeridas para crear licitaciones.
/// </summary>
public interface ILicitacionRepository
{
    /// <summary>
    /// Determina si existe una licitación con el código normalizado indicado.
    /// </summary>
    /// <param name="codigoNormalizado">Código normalizado que se desea buscar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns><see langword="true" /> si el código ya existe; de lo contrario, <see langword="false" />.</returns>
    Task<bool> ExisteConCodigoNormalizadoAsync(
        string codigoNormalizado,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega y guarda una licitación.
    /// </summary>
    /// <param name="licitacion">Licitación que se desea persistir.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="LicitacionDuplicadaException">El código normalizado ya está registrado.</exception>
    Task AgregarAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default);
}
