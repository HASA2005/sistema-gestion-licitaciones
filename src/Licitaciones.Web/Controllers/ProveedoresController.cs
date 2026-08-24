using Licitaciones.Web.Models.Proveedores;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("proveedores")]
public sealed class ProveedoresController : Controller
{
    [HttpGet("registrar")]
    public IActionResult Registrar()
    {
        return View(new RegistrarProveedorViewModel());
    }
}
