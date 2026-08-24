using System.Globalization;
using System.Text.RegularExpressions;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.EndToEndTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit.Abstractions;

namespace Licitaciones.EndToEndTests.Web.Licitaciones;

public sealed class FlujoLicitacionE2eTests(
    LicitacionesE2eFixture aplicacion,
    ITestOutputHelper salida) : PageTest,
    IClassFixture<LicitacionesE2eFixture>
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-CR",
            TimezoneId = "America/Costa_Rica"
        };
    }

    [Fact]
    public async Task CrearYPublicar_DesdeNavegador_PersisteLicitacionPublicada()
    {
        var codigo = $"E2E-{Guid.NewGuid():N}".ToUpperInvariant();
        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            "America/Costa_Rica");
        var fechaCierreLocal = TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow.AddDays(7),
            zonaHoraria);

        try
        {
            await Page.GotoAsync(
                new Uri(aplicacion.DireccionBase, "/licitaciones/crear")
                    .AbsoluteUri);
            await Expect(Page.GetByRole(
                    AriaRole.Heading,
                    new() { Name = "Crear licitación" }))
                .ToBeVisibleAsync();

            await Page.GetByLabel("Código", new() { Exact = false })
                .FillAsync(codigo);
            await Page.GetByLabel("Título", new() { Exact = false })
                .FillAsync("Compra automatizada E2E");
            await Page.GetByLabel(
                    "Presupuesto estimado (CRC)",
                    new() { Exact = false })
                .FillAsync("1500000.50");
            await Page.Locator("#FechaCierreLocal").FillAsync(
                fechaCierreLocal.ToString(
                    "yyyy-MM-ddTHH:mm",
                    CultureInfo.InvariantCulture));

            await Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Guardar borrador" })
                .ClickAsync();

            await Expect(Page).ToHaveURLAsync(
                new Regex(
                    @"/licitaciones/[0-9a-f-]+/publicar$",
                    RegexOptions.IgnoreCase));
            await Expect(Page.GetByText(
                    "Licitación creada correctamente.",
                    new() { Exact = true }))
                .ToBeVisibleAsync();
            await Expect(Page.Locator("dl").GetByText(
                    "Borrador",
                    new() { Exact = true }))
                .ToBeVisibleAsync();

            await Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Publicar licitación" })
                .ClickAsync();

            await Expect(Page.GetByText(
                    "Licitación publicada correctamente.",
                    new() { Exact = true }))
                .ToBeVisibleAsync();
            await Expect(Page.Locator("dl").GetByText(
                    "Publicada",
                    new() { Exact = true }))
                .ToBeVisibleAsync();
            await Expect(Page.GetByRole(
                    AriaRole.Button,
                    new() { Name = "Publicar licitación" }))
                .ToHaveCountAsync(0);

            await using var contexto = aplicacion.CrearContexto();
            var guardada = await contexto.Licitaciones
                .AsNoTracking()
                .SingleAsync(licitacion =>
                    licitacion.CodigoNormalizado == codigo);
            Assert.Equal(EstadoLicitacion.Publicada, guardada.Estado);
            Assert.Equal(TimeSpan.Zero, guardada.UpdatedAt.Offset);
        }
        catch
        {
            salida.WriteLine(aplicacion.ObtenerRegistros());
            await GuardarCapturaAsync();
            throw;
        }
    }

    private async Task GuardarCapturaAsync()
    {
        try
        {
            Directory.CreateDirectory(aplicacion.DirectorioEvidencias);
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(
                    aplicacion.DirectorioEvidencias,
                    "flujo-licitacion-fallo.png"),
                FullPage = true
            });
        }
        catch (PlaywrightException excepcion)
        {
            salida.WriteLine(
                $"No fue posible guardar la captura E2E: {excepcion.Message}");
        }
    }
}
