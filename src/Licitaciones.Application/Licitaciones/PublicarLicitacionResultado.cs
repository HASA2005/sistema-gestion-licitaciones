using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Contiene la representación producida al publicar una licitación.
/// </summary>
/// <param name="Id">Identificador de la licitación.</param>
/// <param name="Codigo">Código visible de la licitación.</param>
/// <param name="Titulo">Título de la licitación.</param>
/// <param name="PresupuestoEstimadoCrc">Presupuesto estimado en colones.</param>
/// <param name="FechaCierre">Fecha de cierre expresada en UTC.</param>
/// <param name="Estado">Estado resultante.</param>
/// <param name="UpdatedAt">Instante UTC de la publicación.</param>
/// <param name="Mensaje">Confirmación para el usuario.</param>
public sealed record PublicarLicitacionResultado(
    Guid Id,
    string Codigo,
    string Titulo,
    decimal PresupuestoEstimadoCrc,
    DateTimeOffset FechaCierre,
    EstadoLicitacion Estado,
    DateTimeOffset UpdatedAt,
    string Mensaje);
