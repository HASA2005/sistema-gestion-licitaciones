using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Define las operaciones de persistencia requeridas por los casos de uso de licitaciones.
/// </summary>
public interface ILicitacionRepository
{
    Task<IReadOnlyList<Licitacion>> ListarAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Licitacion>>(Array.Empty<Licitacion>());
    Task EliminarAsync(Licitacion licitacion, CancellationToken cancellationToken = default) => Task.CompletedTask;
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

    /// <summary>
    /// Obtiene una licitación con seguimiento para modificarla de forma segura.
    /// </summary>
    /// <param name="licitacionId">Identificador que se desea consultar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>La licitación encontrada o <see langword="null" />.</returns>
    Task<Licitacion?> ObtenerPorIdAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda los cambios de una licitación previamente cargada.
    /// </summary>
    /// <param name="licitacion">Licitación modificada.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Una tarea que representa la operación asíncrona.</returns>
    /// <exception cref="LicitacionConcurrenciaException">Otra operación modificó la misma fila.</exception>
    Task GuardarCambiosAsync(
        Licitacion licitacion,
        CancellationToken cancellationToken = default);
}
