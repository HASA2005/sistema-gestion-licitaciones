using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Web.Models.Ofertas;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;
[Route("ofertas")]
public sealed class OfertasController(
    OfertaService service,
    ILicitacionRepository licitaciones,
    IProveedorRepository proveedores) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(Guid? licitacionId, Guid? proveedorId, CancellationToken ct)
    {
        var ofertas = await service.ListarAsync(licitacionId, proveedorId, ct);
        var licitacionesDisponibles = await licitaciones.ListarAsync(ct);
        var proveedoresDisponibles = await proveedores.ListarAsync(ct);
        var licitacionesPorId = licitacionesDisponibles.ToDictionary(x => x.Id);
        var proveedoresPorId = proveedoresDisponibles.ToDictionary(x => x.Id);

        var modelo = ofertas
            .Where(x => licitacionesPorId.ContainsKey(x.LicitacionId) && proveedoresPorId.ContainsKey(x.ProveedorId))
            .Select(x => new OfertaListadoViewModel(
                x.Id,
                licitacionesPorId[x.LicitacionId].Codigo,
                proveedoresPorId[x.ProveedorId].Nombre,
                x.MontoCrc,
                x.CreatedAt))
            .ToList();

        return View(modelo);
    }

    [HttpGet("licitacion/{licitacionId:guid}/mejor")]
    public async Task<IActionResult> Mejor(Guid licitacionId, CancellationToken ct) =>
        View(await service.MejorAsync(licitacionId, ct));

    [HttpGet("crear")]
    public async Task<IActionResult> Crear(Guid licitacionId, CancellationToken ct)
    {
        var model = new OfertaViewModel { LicitacionId = licitacionId };
        await CargarOpcionesAsync(model, ct);
        return View(model);
    }

    [HttpPost("crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(OfertaViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await CargarOpcionesAsync(model, ct);
            return View(model);
        }
        try
        {
            await service.CrearAsync(model.LicitacionId, model.ProveedorId, model.MontoCrc, ct);
            return RedirectToAction(nameof(Index), new { licitacionId = model.LicitacionId });
        }
        catch (Exception e) when (e is OfertaReglaException or OfertaDuplicadaException or ArgumentException)
        {
            ModelState.AddModelError(string.Empty, e.Message);
            await CargarOpcionesAsync(model, ct);
            return View(model);
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detalle(Guid id, CancellationToken ct)
    {
        try
        {
            var oferta = await service.ObtenerAsync(id, ct);
            var licitacion = await licitaciones.ObtenerPorIdAsync(oferta.LicitacionId, ct);
            var proveedor = await proveedores.ObtenerPorIdAsync(oferta.ProveedorId, ct);

            if (licitacion is null || proveedor is null) return NotFound();

            return View(new OfertaDetalleViewModel(
                oferta.Id,
                licitacion.Codigo,
                licitacion.Titulo,
                proveedor.Nombre,
                oferta.MontoCrc,
                oferta.CreatedAt,
                oferta.UpdatedAt));
        }
        catch (OfertaNoEncontradaException) { return NotFound(); }
    }

    [HttpGet("{id:guid}/editar")]
    public async Task<IActionResult> Editar(Guid id, CancellationToken ct)
    {
        try
        {
            var oferta = await service.ObtenerAsync(id, ct);
            var model = new OfertaViewModel { LicitacionId = oferta.LicitacionId, ProveedorId = oferta.ProveedorId, MontoCrc = oferta.MontoCrc };
            await CargarOpcionesAsync(model, ct);
            return View(model);
        }
        catch (OfertaNoEncontradaException) { return NotFound(); }
    }

    [HttpPost("{id:guid}/editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, OfertaViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await CargarOpcionesAsync(model, ct);
            return View(model);
        }
        try { await service.EditarAsync(id, model.MontoCrc, ct); return RedirectToAction(nameof(Detalle), new { id }); }
        catch (Exception e) when (e is OfertaReglaException or OfertaNoEncontradaException or ArgumentException) { ModelState.AddModelError(string.Empty, e.Message); await CargarOpcionesAsync(model, ct); return View(model); }
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

    private async Task CargarOpcionesAsync(OfertaViewModel model, CancellationToken ct)
    {
        var licitacionesDisponibles = await licitaciones.ListarAsync(ct);
        model.Licitaciones = licitacionesDisponibles
            .Where(x => x.Estado == EstadoLicitacion.Publicada)
            .OrderBy(x => x.Codigo)
            .Select(x => new LicitacionOfertaOption(x.Id, x.Codigo, x.Titulo))
            .ToList();

        model.Proveedores = (await proveedores.ListarAsync(ct))
            .OrderBy(x => x.Nombre)
            .Select(x => new ProveedorOfertaOption(x.Id, x.Nombre))
            .ToList();
    }
}
