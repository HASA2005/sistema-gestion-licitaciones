using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Web.Models.Licitaciones;
using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Web.Controllers;

/// <summary>
/// Atiende el formulario web para crear licitaciones en estado Borrador.
/// </summary>
/// <param name="crearLicitacion">Servicio de aplicación que crea la licitación.</param>
[Route("licitaciones")]
public sealed class LicitacionesController(
    CrearLicitacionService crearLicitacion,
    PublicarLicitacionService publicarLicitacion) : Controller
{
    private const string IdZonaHorariaCostaRica = "America/Costa_Rica";

    /// <summary>
    /// Muestra el formulario para crear una licitación.
    /// </summary>
    /// <returns>La vista del formulario con un modelo vacío.</returns>
    [HttpGet("crear")]
    public IActionResult Crear()
    {
        return View(new CrearLicitacionViewModel());
    }

    /// <summary>
    /// Valida y procesa el formulario para crear una licitación.
    /// </summary>
    /// <param name="modelo">Datos enviados desde el formulario.</param>
    /// <param name="cancellationToken">Token para cancelar la operación asíncrona.</param>
    /// <returns>
    /// Una redirección al formulario cuando la creación tiene éxito, o la vista con errores de validación.
    /// </returns>
    [HttpPost("crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
        CrearLicitacionViewModel modelo,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            var comando = new CrearLicitacionComando(
                modelo.Codigo,
                modelo.Titulo,
                modelo.PresupuestoEstimadoCrc!.Value,
                ConvertirFechaLocalAUtc(modelo.FechaCierreLocal!.Value));

            var resultado = await crearLicitacion.EjecutarAsync(
                comando,
                cancellationToken);

            TempData["MensajeExito"] = resultado.Mensaje;
            return RedirectToAction(
                nameof(Publicar),
                new { id = resultado.Id });
        }
        catch (LicitacionDuplicadaException excepcion)
        {
            ModelState.AddModelError(nameof(modelo.Codigo), excepcion.Message);
            return View(modelo);
        }
        catch (ArgumentException excepcion)
        {
            var campo = ObtenerCampoDelModelo(excepcion.ParamName);
            var mensaje = excepcion.Message.Split(Environment.NewLine)[0];
            ModelState.AddModelError(campo, mensaje);
            return View(modelo);
        }
    }

    /// <summary>
    /// Muestra los datos de la licitación antes de publicarla.
    /// </summary>
    /// <param name="id">Identificador de la licitación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La vista de confirmación o un resultado 404.</returns>
    [HttpGet("{id:guid}/publicar")]
    public async Task<IActionResult> Publicar(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await publicarLicitacion.ConsultarAsync(
                id,
                cancellationToken);

            return View(CrearModeloPublicacion(resultado));
        }
        catch (LicitacionNoEncontradaException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Confirma la publicación de una licitación en estado Borrador.
    /// </summary>
    /// <param name="id">Identificador de la licitación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una redirección a la confirmación o un resultado 404.</returns>
    [HttpPost("{id:guid}/publicar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublicarConfirmado(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await publicarLicitacion.EjecutarAsync(
                id,
                cancellationToken);

            TempData["MensajeExito"] = resultado.Mensaje;
        }
        catch (LicitacionNoEncontradaException)
        {
            return NotFound();
        }
        catch (PublicacionLicitacionInvalidaException excepcion)
        {
            TempData["MensajeError"] = excepcion.Message;
        }
        catch (LicitacionConcurrenciaException excepcion)
        {
            TempData["MensajeError"] = excepcion.Message;
        }

        return RedirectToAction(nameof(Publicar), new { id });
    }

    private static DateTimeOffset ConvertirFechaLocalAUtc(DateTime fechaLocal)
    {
        var fechaSinZona = DateTime.SpecifyKind(
            fechaLocal,
            DateTimeKind.Unspecified);
        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            IdZonaHorariaCostaRica);

        if (zonaHoraria.IsInvalidTime(fechaSinZona) ||
            zonaHoraria.IsAmbiguousTime(fechaSinZona))
        {
            throw new ArgumentException(
                "La fecha y hora de cierre no es válida en la zona horaria de Costa Rica.",
                nameof(fechaLocal));
        }

        var fechaUtc = TimeZoneInfo.ConvertTimeToUtc(
            fechaSinZona,
            zonaHoraria);

        return new DateTimeOffset(fechaUtc);
    }

    private static string ObtenerCampoDelModelo(string? nombreParametro)
    {
        return nombreParametro?.ToUpperInvariant() switch
        {
            "CODIGO" => nameof(CrearLicitacionViewModel.Codigo),
            "TITULO" => nameof(CrearLicitacionViewModel.Titulo),
            "PRESUPUESTO" or
            "PRESUPUESTOESTIMADO" or
            "PRESUPUESTOESTIMADOCRC" =>
                nameof(CrearLicitacionViewModel.PresupuestoEstimadoCrc),
            "FECHACIERRE" or
            "FECHALOCAL" or
            "FECHACIERRELOCAL" or
            "FECHACIERREUTC" =>
                nameof(CrearLicitacionViewModel.FechaCierreLocal),
            _ => string.Empty
        };
    }

    private static PublicarLicitacionViewModel CrearModeloPublicacion(
        LicitacionParaPublicarResultado resultado)
    {
        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            IdZonaHorariaCostaRica);

        return new PublicarLicitacionViewModel
        {
            Id = resultado.Id,
            Codigo = resultado.Codigo,
            Titulo = resultado.Titulo,
            PresupuestoEstimadoCrc = resultado.PresupuestoEstimadoCrc,
            FechaCierreCostaRica = TimeZoneInfo.ConvertTime(
                resultado.FechaCierre,
                zonaHoraria),
            Estado = resultado.Estado,
            PuedePublicarse = resultado.Estado == EstadoLicitacion.Borrador
        };
    }
}
