using System.Text;

namespace Licitaciones.Domain.Licitaciones;

/// <summary>
/// Representa una licitación y protege las reglas válidas desde su creación.
/// </summary>
public sealed class Licitacion
{
    private const decimal PresupuestoMaximoCrc = 9_999_999_999_999_999.99m;

    /// <summary>
    /// Obtiene la cantidad máxima de caracteres permitida para el código.
    /// </summary>
    public const int LongitudMaximaCodigo = 100;

    /// <summary>
    /// Obtiene la cantidad máxima de caracteres permitida para el título.
    /// </summary>
    public const int LongitudMaximaTitulo = 200;

    private Licitacion()
    {
        Codigo = string.Empty;
        CodigoNormalizado = string.Empty;
        Titulo = string.Empty;
    }

    /// <summary>
    /// Inicializa una licitación en estado <see cref="EstadoLicitacion.Borrador" />
    /// usando la fecha y hora actuales como momento de creación.
    /// </summary>
    /// <param name="codigo">Código único de la licitación.</param>
    /// <param name="titulo">Título descriptivo de la licitación.</param>
    /// <param name="presupuestoEstimadoCrc">Presupuesto estimado en colones costarricenses.</param>
    /// <param name="fechaCierre">Fecha y hora de cierre.</param>
    /// <exception cref="ArgumentException">
    /// Alguno de los datos no cumple las reglas de creación de la licitación.
    /// </exception>
    public Licitacion(
        string codigo,
        string titulo,
        decimal presupuestoEstimadoCrc,
        DateTimeOffset fechaCierre)
        : this(
            codigo,
            titulo,
            presupuestoEstimadoCrc,
            fechaCierre,
            TimeProvider.System.GetUtcNow())
    {
    }

    /// <summary>
    /// Inicializa una licitación en estado <see cref="EstadoLicitacion.Borrador" />
    /// con un momento de creación específico.
    /// </summary>
    /// <param name="codigo">Código único de la licitación.</param>
    /// <param name="titulo">Título descriptivo de la licitación.</param>
    /// <param name="presupuestoEstimadoCrc">Presupuesto estimado en colones costarricenses.</param>
    /// <param name="fechaCierre">Fecha y hora de cierre.</param>
    /// <param name="fechaCreacion">Fecha y hora que se registrará como creación.</param>
    /// <exception cref="ArgumentException">
    /// Alguno de los datos no cumple las reglas de creación de la licitación.
    /// </exception>
    public Licitacion(
        string codigo,
        string titulo,
        decimal presupuestoEstimadoCrc,
        DateTimeOffset fechaCierre,
        DateTimeOffset fechaCreacion)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException(
                "El código de la licitación es obligatorio.",
                nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(titulo))
        {
            throw new ArgumentException(
                "El título de la licitación es obligatorio.",
                nameof(titulo));
        }

        var codigoLimpio = codigo
            .Normalize(NormalizationForm.FormC)
            .Trim();
        var tituloLimpio = titulo
            .Normalize(NormalizationForm.FormC)
            .Trim();

        if (codigoLimpio.Length > LongitudMaximaCodigo)
        {
            throw new ArgumentException(
                $"El código de la licitación no puede superar los {LongitudMaximaCodigo} caracteres.",
                nameof(codigo));
        }

        if (tituloLimpio.Length > LongitudMaximaTitulo)
        {
            throw new ArgumentException(
                $"El título de la licitación no puede superar los {LongitudMaximaTitulo} caracteres.",
                nameof(titulo));
        }

        if (codigoLimpio.Any(char.IsControl))
        {
            throw new ArgumentException(
                "El código de la licitación no puede contener caracteres de control.",
                nameof(codigo));
        }

        if (tituloLimpio.Any(char.IsControl))
        {
            throw new ArgumentException(
                "El título de la licitación no puede contener caracteres de control.",
                nameof(titulo));
        }

        if (presupuestoEstimadoCrc <= 0)
        {
            throw new ArgumentException(
                "El presupuesto estimado debe ser mayor que cero.",
                nameof(presupuestoEstimadoCrc));
        }

        if (decimal.Round(presupuestoEstimadoCrc, 2) != presupuestoEstimadoCrc)
        {
            throw new ArgumentException(
                "El presupuesto estimado no puede tener más de dos decimales.",
                nameof(presupuestoEstimadoCrc));
        }

        if (presupuestoEstimadoCrc > PresupuestoMaximoCrc)
        {
            throw new ArgumentException(
                "El presupuesto estimado supera el monto máximo permitido.",
                nameof(presupuestoEstimadoCrc));
        }

        if (fechaCierre == default)
        {
            throw new ArgumentException(
                "La fecha de cierre es obligatoria.",
                nameof(fechaCierre));
        }

        Id = Guid.NewGuid();
        Codigo = codigoLimpio;
        CodigoNormalizado = Codigo.ToUpperInvariant();
        Titulo = tituloLimpio;
        PresupuestoEstimadoCrc = presupuestoEstimadoCrc;
        FechaCierre = fechaCierre.ToUniversalTime();
        Estado = EstadoLicitacion.Borrador;
        CreatedAt = fechaCreacion.ToUniversalTime();
        UpdatedAt = CreatedAt;
    }

    /// <summary>
    /// Obtiene el identificador único de la licitación.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Obtiene el código de la licitación sin espacios exteriores.
    /// </summary>
    public string Codigo { get; private set; }

    /// <summary>
    /// Obtiene el código normalizado utilizado para comprobar unicidad.
    /// </summary>
    public string CodigoNormalizado { get; private set; }

    /// <summary>
    /// Obtiene el título de la licitación.
    /// </summary>
    public string Titulo { get; private set; }

    /// <summary>
    /// Obtiene el presupuesto estimado en colones costarricenses.
    /// </summary>
    public decimal PresupuestoEstimadoCrc { get; private set; }

    /// <summary>
    /// Obtiene la fecha y hora de cierre expresada en UTC.
    /// </summary>
    public DateTimeOffset FechaCierre { get; private set; }

    /// <summary>
    /// Obtiene el estado actual de la licitación.
    /// </summary>
    public EstadoLicitacion Estado { get; private set; }

    /// <summary>
    /// Obtiene la fecha y hora de creación expresada en UTC.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Obtiene la fecha y hora de la última actualización expresada en UTC.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Obtiene la versión de concurrencia administrada por PostgreSQL.
    /// </summary>
    public uint Version { get; private set; }
}
