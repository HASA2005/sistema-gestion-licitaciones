using Licitaciones.Application.Proveedores;
using Licitaciones.Web.Models.Proveedores;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

[Route("proveedores")]
public sealed class ProveedoresController(
    RegistrarProveedorService registrarProveedor) : Controller
{
    [HttpGet("registrar")]
    public IActionResult Registrar()
    {
        return View(new RegistrarProveedorViewModel());
    }

    [HttpPost("registrar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(
        RegistrarProveedorViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await registrarProveedor.EjecutarAsync(
            modelo.Nombre,
            cancellationToken);

        TempData["MensajeExito"] = resultado.Mensaje;
        return RedirectToAction(nameof(Registrar));
    }
}
