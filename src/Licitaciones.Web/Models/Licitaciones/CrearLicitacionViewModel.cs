using System.ComponentModel.DataAnnotations;
using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Web.Models.Licitaciones;

/// <summary>
/// Representa los campos del formulario para crear una licitación.
/// </summary>
public sealed class CrearLicitacionViewModel
{
    /// <summary>
    /// Obtiene o establece el código único de la licitación.
    /// </summary>
    [Required(ErrorMessage = "El código de la licitación es obligatorio.")]
    [StringLength(
        Licitacion.LongitudMaximaCodigo,
        ErrorMessage = "El código de la licitación no puede superar los 100 caracteres.")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el título descriptivo de la licitación.
    /// </summary>
    [Required(ErrorMessage = "El título de la licitación es obligatorio.")]
    [StringLength(
        Licitacion.LongitudMaximaTitulo,
        ErrorMessage = "El título de la licitación no puede superar los 200 caracteres.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el presupuesto estimado en colones costarricenses.
    /// </summary>
    [Required(ErrorMessage = "El presupuesto estimado es obligatorio.")]
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "El presupuesto estimado debe ser mayor que cero y no superar el máximo permitido.")]
    [Display(Name = "Presupuesto estimado (CRC)")]
    public decimal? PresupuestoEstimadoCrc { get; set; }

    /// <summary>
    /// Obtiene o establece la fecha y hora de cierre en la zona horaria de Costa Rica.
    /// </summary>
    [Required(ErrorMessage = "La fecha y hora de cierre es obligatoria.")]
    [Display(Name = "Fecha y hora de cierre")]
    public DateTime? FechaCierreLocal { get; set; }
}
