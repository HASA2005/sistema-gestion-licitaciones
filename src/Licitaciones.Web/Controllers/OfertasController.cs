using Licitaciones.Application.Ofertas;
using Licitaciones.Web.Models.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;
[Route("ofertas")]
public sealed class OfertasController(OfertaService service) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? licitacionId, Guid? proveedorId, CancellationToken ct) =>
        View(await service.ListarAsync(licitacionId, proveedorId, ct));

    [HttpGet("licitacion/{licitacionId:guid}/mejor")]
    public async Task<IActionResult> Mejor(Guid licitacionId, CancellationToken ct) =>
        View(await service.MejorAsync(licitacionId, ct));

    [HttpGet("crear")]
    public IActionResult Crear(Guid licitacionId) =>
        View(new OfertaViewModel { LicitacionId = licitacionId });

    [HttpPost("crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(OfertaViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await service.CrearAsync(model.LicitacionId, model.ProveedorId, model.MontoCrc, ct);
            return RedirectToAction(nameof(Index), new { licitacionId = model.LicitacionId });
        }
        catch (Exception e) when (e is OfertaReglaException or OfertaDuplicadaException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            return View(model);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detalle(Guid id, CancellationToken ct)
    {
        try { return View(await service.ObtenerAsync(id, ct)); }
        catch (OfertaNoEncontradaException) { return NotFound(); }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id, CancellationToken ct)
    {
        try
        {
            var oferta = await service.ObtenerAsync(id, ct);
            return View(new OfertaViewModel { LicitacionId = oferta.LicitacionId, ProveedorId = oferta.ProveedorId, MontoCrc = oferta.MontoCrc });
        }
        catch (OfertaNoEncontradaException) { return NotFound(); }
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, OfertaViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try { await service.EditarAsync(id, model.MontoCrc, ct); return RedirectToAction(nameof(Detalle), new { id }); }
        catch (Exception e) when (e is OfertaReglaException or OfertaNoEncontradaException or ArgumentException) { ModelState.AddModelError(string.Empty, e.Message); return View(model); }
    }

    [HttpPost("{id:guid}/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken ct)
    {
        try { await service.EliminarAsync(id, ct); return RedirectToAction(nameof(Index)); }
        catch (OfertaNoEncontradaException) { return NotFound(); }
        catch (OfertaReglaException e)
        {
            TempData["MensajeError"] = e.Message;
            return RedirectToAction(nameof(Detalle), new { id });
        }
    }
}
