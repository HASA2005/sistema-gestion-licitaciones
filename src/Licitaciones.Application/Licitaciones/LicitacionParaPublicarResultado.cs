using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Contiene los datos de solo lectura necesarios para confirmar una publicación.
/// </summary>
/// <param name="Id">Identificador de la licitación.</param>
/// <param name="Codigo">Código visible.</param>
/// <param name="Titulo">Título.</param>
/// <param name="PresupuestoEstimadoCrc">Presupuesto estimado en colones.</param>
/// <param name="FechaCierre">Fecha de cierre expresada en UTC.</param>
/// <param name="Estado">Estado actual.</param>
public sealed record LicitacionParaPublicarResultado(
    Guid Id,
    string Codigo,
    string Titulo,
    decimal PresupuestoEstimadoCrc,
    DateTimeOffset FechaCierre,
    EstadoLicitacion Estado);
