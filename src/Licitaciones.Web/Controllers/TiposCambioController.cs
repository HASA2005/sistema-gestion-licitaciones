using Licitaciones.Application.TiposCambio;
using Licitaciones.Web.Models.TiposCambio;
using Microsoft.AspNetCore.Mvc;
namespace Licitaciones.Web.Controllers;
[Route("tipos-cambio")]
public sealed class TiposCambioController(TipoCambioService s) : Controller
{
    [HttpGet("")] public async Task<IActionResult> Index(CancellationToken c) => View(await s.ListarAsync(c));
    [HttpGet("crear")] public IActionResult Crear() => View(new TipoCambioViewModel());
    [HttpPost("crear")][ValidateAntiForgeryToken] public async Task<IActionResult> Crear(TipoCambioViewModel m, CancellationToken c) { if (!ModelState.IsValid) return View(m); try { await s.CrearAsync(m.CrcPorUsd, m.Activo, c); return RedirectToAction(nameof(Index)); } catch (Exception e) when (e is ArgumentException or InvalidOperationException) { ModelState.AddModelError("", e.Message); return View(m); } }
    [HttpGet("{id:guid}/editar")] public async Task<IActionResult> Editar(Guid id, CancellationToken c) { try { var t = await s.ObtenerAsync(id, c); return View(new TipoCambioViewModel { CrcPorUsd = t.CrcPorUsd, Activo = t.Activo }); } catch (KeyNotFoundException) { return NotFound(); } }
    [HttpPost("{id:guid}/editar")][ValidateAntiForgeryToken] public async Task<IActionResult> Editar(Guid id, TipoCambioViewModel m, CancellationToken c) { if (!ModelState.IsValid) return View(m); try { await s.EditarAsync(id, m.CrcPorUsd, m.Activo, c); return RedirectToAction(nameof(Index)); } catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException) { ModelState.AddModelError("", e.Message); return View(m); } }
    [HttpPost("{id:guid}/eliminar")][ValidateAntiForgeryToken] public async Task<IActionResult> Eliminar(Guid id, CancellationToken c) { try { await s.EliminarAsync(id, c); } catch (InvalidOperationException e) { TempData["Error"] = e.Message; } catch (KeyNotFoundException) { return NotFound(); } return RedirectToAction(nameof(Index)); }
}
