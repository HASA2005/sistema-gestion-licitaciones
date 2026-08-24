using System.ComponentModel.DataAnnotations;
namespace Licitaciones.Web.Models.Aprobaciones;
public sealed class NivelAprobacionViewModel { [Required] public string Responsable { get; set; } = string.Empty; [Range(typeof(decimal), "0.01", "9999999999999999")] public decimal MontoMinimoCrc { get; set; } [Range(typeof(decimal), "0.01", "9999999999999999")] public decimal? MontoMaximoCrc { get; set; } }
