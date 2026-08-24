using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Api.Endpoints.Licitaciones;

/// <summary>
/// Define los datos aceptados para crear una licitación en Borrador.
/// </summary>
/// <param name="Codigo">Código único de la licitación.</param>
/// <param name="Titulo">Título descriptivo de la licitación.</param>
/// <param name="PresupuestoEstimadoCrc">Presupuesto estimado en colones costarricenses.</param>
/// <param name="FechaCierre">Fecha y hora de cierre con desplazamiento de zona horaria explícito.</param>
public sealed record CrearLicitacionRequest(
    [property: Required]
    [property: StringLength(Licitacion.LongitudMaximaCodigo)]
    [property: RegularExpression(@"^[^\p{Cc}]*$")]
    string? Codigo,
    [property: Required]
    [property: StringLength(Licitacion.LongitudMaximaTitulo)]
    [property: RegularExpression(@"^[^\p{Cc}]*$")]
    string? Titulo,
    [property: Required]
    decimal? PresupuestoEstimadoCrc,
    [property: Required]
    [property: JsonConverter(typeof(FechaCierreConZonaJsonConverter))]
    DateTimeOffset? FechaCierre);
