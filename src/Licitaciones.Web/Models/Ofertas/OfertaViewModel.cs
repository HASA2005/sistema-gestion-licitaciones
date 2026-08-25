using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Ofertas;
public sealed class OfertaViewModel
{
    [Required] public Guid LicitacionId { get; set; }
    [Required] public Guid ProveedorId { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999")] public decimal MontoCrc { get; set; }
    public IReadOnlyList<LicitacionOfertaOption> Licitaciones { get; set; } = [];
    public IReadOnlyList<ProveedorOfertaOption> Proveedores { get; set; } = [];
}

public sealed record LicitacionOfertaOption(Guid Id, string Codigo, string Titulo);
public sealed record ProveedorOfertaOption(Guid Id, string Nombre);
