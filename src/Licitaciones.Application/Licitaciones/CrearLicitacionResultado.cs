using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Representa el resultado de crear una licitación.
/// </summary>
/// <param name="Id">Identificador asignado a la licitación.</param>
/// <param name="Codigo">Código almacenado.</param>
/// <param name="Titulo">Título almacenado.</param>
/// <param name="PresupuestoEstimadoCrc">Presupuesto estimado en colones costarricenses.</param>
/// <param name="FechaCierre">Fecha y hora de cierre expresada en UTC.</param>
/// <param name="Estado">Estado inicial de la licitación.</param>
/// <param name="Mensaje">Mensaje que confirma la operación.</param>
public sealed record CrearLicitacionResultado(
    Guid Id,
    string Codigo,
    string Titulo,
    decimal PresupuestoEstimadoCrc,
    DateTimeOffset FechaCierre,
    EstadoLicitacion Estado,
    string Mensaje);
