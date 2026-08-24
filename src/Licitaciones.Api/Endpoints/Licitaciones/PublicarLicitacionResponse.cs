namespace Licitaciones.Api.Endpoints.Licitaciones;

/// <summary>
/// Representa la confirmación HTTP de una licitación publicada.
/// </summary>
/// <param name="Id">Identificador de la licitación.</param>
/// <param name="Codigo">Código visible.</param>
/// <param name="Titulo">Título.</param>
/// <param name="PresupuestoEstimadoCrc">Presupuesto estimado en colones.</param>
/// <param name="FechaCierre">Fecha de cierre expresada en UTC.</param>
/// <param name="Estado">Estado resultante.</param>
/// <param name="UpdatedAt">Instante UTC de la publicación.</param>
/// <param name="Mensaje">Confirmación para el usuario.</param>
public sealed record PublicarLicitacionResponse(
    Guid Id,
    string Codigo,
    string Titulo,
    decimal PresupuestoEstimadoCrc,
    DateTimeOffset FechaCierre,
    string Estado,
    DateTimeOffset UpdatedAt,
    string Mensaje);
