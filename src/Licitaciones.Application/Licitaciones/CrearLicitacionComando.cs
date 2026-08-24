namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Contiene los datos necesarios para crear una licitación en estado Borrador.
/// </summary>
/// <param name="Codigo">Código único de la licitación.</param>
/// <param name="Titulo">Título descriptivo de la licitación.</param>
/// <param name="PresupuestoEstimadoCrc">Presupuesto estimado en colones costarricenses.</param>
/// <param name="FechaCierre">Fecha y hora de cierre.</param>
public sealed record CrearLicitacionComando(
    string Codigo,
    string Titulo,
    decimal PresupuestoEstimadoCrc,
    DateTimeOffset FechaCierre);
