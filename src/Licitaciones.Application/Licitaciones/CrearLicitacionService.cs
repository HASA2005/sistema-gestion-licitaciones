using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Coordina la validación y persistencia de una nueva licitación en estado Borrador.
/// </summary>
public sealed class CrearLicitacionService
{
    private readonly ILicitacionRepository _repositorio;
    private readonly TimeProvider _reloj;

    /// <summary>
    /// Inicializa el servicio con el reloj del sistema.
    /// </summary>
    /// <param name="repositorio">Repositorio utilizado para consultar y guardar licitaciones.</param>
    public CrearLicitacionService(ILicitacionRepository repositorio)
        : this(repositorio, TimeProvider.System)
    {
    }

    /// <summary>
    /// Inicializa el servicio con un proveedor de tiempo específico.
    /// </summary>
    /// <param name="repositorio">Repositorio utilizado para consultar y guardar licitaciones.</param>
    /// <param name="reloj">Proveedor de tiempo usado para registrar la creación.</param>
    public CrearLicitacionService(
        ILicitacionRepository repositorio,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _reloj = reloj;
    }

    /// <summary>
    /// Crea y persiste una licitación en estado Borrador.
    /// </summary>
    /// <param name="comando">Datos de la licitación que se desea crear.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>El resultado con la representación de la licitación creada.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="comando" /> es <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">Los datos de la licitación son inválidos.</exception>
    /// <exception cref="LicitacionDuplicadaException">Ya existe una licitación con el mismo código.</exception>
    public async Task<CrearLicitacionResultado> EjecutarAsync(
        CrearLicitacionComando comando,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var licitacion = new Licitacion(
            comando.Codigo,
            comando.Titulo,
            comando.PresupuestoEstimadoCrc,
            comando.FechaCierre,
            _reloj.GetUtcNow());

        var existeLicitacion = await _repositorio.ExisteConCodigoNormalizadoAsync(
            licitacion.CodigoNormalizado,
            cancellationToken);

        if (existeLicitacion)
        {
            throw new LicitacionDuplicadaException();
        }

        await _repositorio.AgregarAsync(licitacion, cancellationToken);

        return new CrearLicitacionResultado(
            licitacion.Id,
            licitacion.Codigo,
            licitacion.Titulo,
            licitacion.PresupuestoEstimadoCrc,
            licitacion.FechaCierre,
            licitacion.Estado,
            "Licitación creada correctamente.");
    }
}
