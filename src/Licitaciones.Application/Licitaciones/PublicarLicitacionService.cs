using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Coordina la transición de una licitación de Borrador a Publicada.
/// </summary>
public sealed class PublicarLicitacionService
{
    private readonly ILicitacionRepository _repositorio;
    private readonly TimeProvider _reloj;

    /// <summary>
    /// Consulta los datos necesarios para confirmar la publicación.
    /// </summary>
    /// <param name="licitacionId">Identificador de la licitación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>Los datos actuales de la licitación.</returns>
    /// <exception cref="LicitacionNoEncontradaException">No existe una licitación con el identificador.</exception>
    public async Task<LicitacionParaPublicarResultado> ConsultarAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerRequeridaAsync(
            licitacionId,
            cancellationToken);

        return new LicitacionParaPublicarResultado(
            licitacion.Id,
            licitacion.Codigo,
            licitacion.Titulo,
            licitacion.PresupuestoEstimadoCrc,
            licitacion.FechaCierre,
            licitacion.Estado);
    }

    /// <summary>
    /// Inicializa el servicio con el reloj del sistema.
    /// </summary>
    /// <param name="repositorio">Repositorio utilizado para cargar y guardar la licitación.</param>
    public PublicarLicitacionService(ILicitacionRepository repositorio)
        : this(repositorio, TimeProvider.System)
    {
    }

    /// <summary>
    /// Inicializa el servicio con un proveedor de tiempo específico.
    /// </summary>
    /// <param name="repositorio">Repositorio utilizado para cargar y guardar la licitación.</param>
    /// <param name="reloj">Proveedor del instante actual.</param>
    public PublicarLicitacionService(
        ILicitacionRepository repositorio,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _reloj = reloj;
    }

    /// <summary>
    /// Publica la licitación identificada cuando cumple todas las reglas de negocio.
    /// </summary>
    /// <param name="licitacionId">Identificador de la licitación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>La representación de la licitación publicada.</returns>
    /// <exception cref="LicitacionNoEncontradaException">No existe una licitación con el identificador.</exception>
    /// <exception cref="PublicacionLicitacionInvalidaException">La licitación no puede publicarse.</exception>
    /// <exception cref="LicitacionConcurrenciaException">Otra operación modificó la licitación.</exception>
    public async Task<PublicarLicitacionResultado> EjecutarAsync(
        Guid licitacionId,
        CancellationToken cancellationToken = default)
    {
        var licitacion = await ObtenerRequeridaAsync(
            licitacionId,
            cancellationToken);

        licitacion.Publicar(_reloj.GetUtcNow());
        await _repositorio.GuardarCambiosAsync(
            licitacion,
            cancellationToken);

        return new PublicarLicitacionResultado(
            licitacion.Id,
            licitacion.Codigo,
            licitacion.Titulo,
            licitacion.PresupuestoEstimadoCrc,
            licitacion.FechaCierre,
            licitacion.Estado,
            licitacion.UpdatedAt,
            "Licitación publicada correctamente.");
    }

    private async Task<Licitacion> ObtenerRequeridaAsync(
        Guid licitacionId,
        CancellationToken cancellationToken)
    {
        var licitacion = await _repositorio.ObtenerPorIdAsync(
            licitacionId,
            cancellationToken);

        return licitacion ?? throw new LicitacionNoEncontradaException();
    }
}
