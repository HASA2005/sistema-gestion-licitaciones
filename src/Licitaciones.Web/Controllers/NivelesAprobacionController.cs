using Licitaciones.Application.Aprobaciones;
using Licitaciones.Web.Models.Aprobaciones;
using Microsoft.AspNetCore.Mvc;
namespace Licitaciones.Web.Controllers;
[Route("niveles-aprobacion")]
public sealed class NivelesAprobacionController(NivelAprobacionService s) : Controller
{
    [HttpGet("")] public async Task<IActionResult> Index(CancellationToken c) => View(await s.ListarAsync(c));
    [HttpGet("crear")] public IActionResult Crear() => View(new NivelAprobacionViewModel());
    [HttpPost("crear")][ValidateAntiForgeryToken] public async Task<IActionResult> Crear(NivelAprobacionViewModel m, CancellationToken c) { if (!ModelState.IsValid) return View(m); try { await s.CrearAsync(m.Responsable, m.MontoMinimoCrc, m.MontoMaximoCrc, c); return RedirectToAction(nameof(Index)); } catch (Exception e) when (e is ArgumentException or InvalidOperationException) { ModelState.AddModelError("", e.Message); return View(m); } }
    [HttpGet("{id:guid}/editar")] public async Task<IActionResult> Editar(Guid id, CancellationToken c) { var n = await s.ObtenerAsync(id, c); return n is null ? NotFound() : View(new NivelAprobacionViewModel { Responsable = n.Responsable, MontoMinimoCrc = n.MontoMinimoCrc, MontoMaximoCrc = n.MontoMaximoCrc }); }
    [HttpPost("{id:guid}/editar")][ValidateAntiForgeryToken] public async Task<IActionResult> Editar(Guid id, NivelAprobacionViewModel m, CancellationToken c) { if (!ModelState.IsValid) return View(m); try { await s.EditarAsync(id, m.Responsable, m.MontoMinimoCrc, m.MontoMaximoCrc, c); return RedirectToAction(nameof(Index)); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException) { ModelState.AddModelError("", e.Message); return View(m); } }
    [HttpPost("{id:guid}/eliminar")][ValidateAntiForgeryToken] public async Task<IActionResult> Eliminar(Guid id, CancellationToken c) { await s.EliminarAsync(id, c); return RedirectToAction(nameof(Index)); }
}
