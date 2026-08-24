using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Web.Models.Licitaciones;

/// <summary>
/// Presenta los datos de solo lectura para confirmar una publicación.
/// </summary>
public sealed class PublicarLicitacionViewModel
{
    /// <summary>Identificador de la licitación.</summary>
    public Guid Id { get; init; }

    /// <summary>Código visible de la licitación.</summary>
    public required string Codigo { get; init; }

    /// <summary>Título de la licitación.</summary>
    public required string Titulo { get; init; }

    /// <summary>Presupuesto estimado en colones.</summary>
    public decimal PresupuestoEstimadoCrc { get; init; }

    /// <summary>Fecha de cierre en la zona horaria de Costa Rica.</summary>
    public DateTimeOffset FechaCierreCostaRica { get; init; }

    /// <summary>Estado actual.</summary>
    public EstadoLicitacion Estado { get; init; }

    /// <summary>Indica si la interfaz debe ofrecer la acción de publicar.</summary>
    public bool PuedePublicarse { get; init; }
}
