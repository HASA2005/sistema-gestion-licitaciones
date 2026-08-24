using System.ComponentModel.DataAnnotations;

namespace Licitaciones.Web.Models.Proveedores;

public sealed class RegistrarProveedorViewModel
{
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;
}
