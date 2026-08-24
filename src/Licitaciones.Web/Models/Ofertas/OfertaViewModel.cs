using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Ofertas;
public sealed class OfertaViewModel
{
    [Required] public Guid LicitacionId { get; set; }
    [Required] public Guid ProveedorId { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999")] public decimal MontoCrc { get; set; }
}
