using System.ComponentModel.DataAnnotations;
namespace Licitaciones.Web.Models.Crud;
public sealed class EditarProveedorViewModel { public Guid Id { get; set; } [Required] public string Nombre { get; set; } = ""; }
