using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Web.Models.Crud;
using Microsoft.AspNetCore.Mvc;
namespace Licitaciones.Web.Controllers;
[Route("gestion")]
public sealed class CrudController(GestionarLicitacionesService licitaciones, GestionarProveedoresService proveedores) : Controller
{
    [HttpGet("licitaciones")] public async Task<IActionResult> Licitaciones(CancellationToken c) => View(await licitaciones.ListarAsync(c));
    [HttpGet("licitaciones/{id:guid}")] public async Task<IActionResult> Licitacion(Guid id, CancellationToken c) => View(await licitaciones.ObtenerAsync(id, c));
    [HttpGet("licitaciones/{id:guid}/editar")] public async Task<IActionResult> EditarLicitacion(Guid id, CancellationToken c) { var x = await licitaciones.ObtenerAsync(id, c); return View(new EditarLicitacionViewModel { Id = x.Id, Codigo = x.Codigo, Titulo = x.Titulo, PresupuestoEstimadoCrc = x.PresupuestoEstimadoCrc, FechaCierre = x.FechaCierre }); }
    [HttpPost("licitaciones/{id:guid}/editar")][ValidateAntiForgeryToken] public async Task<IActionResult> EditarLicitacion(Guid id, EditarLicitacionViewModel m, CancellationToken c) { if (!ModelState.IsValid) return View(m); try { await licitaciones.EditarAsync(id, m.Codigo, m.Titulo, m.PresupuestoEstimadoCrc, m.FechaCierre, c); return RedirectToAction(nameof(Licitacion), new { id }); } catch (Exception e) when (e is ArgumentException or InvalidOperationException) { ModelState.AddModelError("", e.Message); return View(m); } }
    [HttpPost("licitaciones/{id:guid}/eliminar")][ValidateAntiForgeryToken] public async Task<IActionResult> EliminarLicitacion(Guid id, CancellationToken c) { try { await licitaciones.EliminarAsync(id, c); return RedirectToAction(nameof(Licitaciones)); } catch (InvalidOperationException e) { TempData["MensajeError"] = e.Message; return RedirectToAction(nameof(Licitacion), new { id }); } }
    [HttpGet("proveedores")] public async Task<IActionResult> Proveedores(CancellationToken c) => View(await proveedores.ListarAsync(c));
    [HttpGet("proveedores/{id:guid}")] public async Task<IActionResult> Proveedor(Guid id, CancellationToken c) => View(await proveedores.ObtenerAsync(id, c));
    [HttpGet("proveedores/{id:guid}/editar")] public async Task<IActionResult> EditarProveedor(Guid id, CancellationToken c) { var x = await proveedores.ObtenerAsync(id, c); return View(new EditarProveedorViewModel { Id = x.Id, Nombre = x.Nombre }); }
    [HttpPost("proveedores/{id:guid}/editar")][ValidateAntiForgeryToken] public async Task<IActionResult> EditarProveedor(Guid id, EditarProveedorViewModel m, CancellationToken c) { if (!ModelState.IsValid) return View(m); try { await proveedores.EditarAsync(id, m.Nombre, c); return RedirectToAction(nameof(Proveedor), new { id }); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or ProveedorDuplicadoException) { ModelState.AddModelError("", e.Message); return View(m); } }
    [HttpPost("proveedores/{id:guid}/eliminar")][ValidateAntiForgeryToken] public async Task<IActionResult> EliminarProveedor(Guid id, CancellationToken c) { try { await proveedores.EliminarAsync(id, c); return RedirectToAction(nameof(Proveedores)); } catch (InvalidOperationException e) { TempData["MensajeError"] = e.Message; return RedirectToAction(nameof(Proveedor), new { id }); } }
}
