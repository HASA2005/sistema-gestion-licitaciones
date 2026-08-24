using System.Net;
using System.Text.RegularExpressions;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Web.Licitaciones;

public sealed class CrearLicitacionWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    [Fact]
    public async Task Post_ConInfraestructuraReal_PersisteBorradorEnUtc()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("licitaciones")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await postgres.StartAsync();

        var opciones = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using (var contexto = new LicitacionesDbContext(opciones))
        {
            await contexto.Database.MigrateAsync();
        }

        await using var aplicacion = new WebFactory(
            postgres.GetConnectionString());
        using var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true
            });

        var paginaFormulario = await cliente.GetAsync("/licitaciones/crear");
        paginaFormulario.EnsureSuccessStatusCode();
        var formulario = await paginaFormulario.Content.ReadAsStringAsync();
        var valores = new List<KeyValuePair<string, string>>
        {
            new("Codigo", "  lic-2030-001  "),
            new("Titulo", "  Compra de equipo informático  "),
            new("PresupuestoEstimadoCrc", "1250000.50"),
            new("FechaCierreLocal", "2030-10-15T18:30"),
            new("__Invariant", "PresupuestoEstimadoCrc"),
            new("__Invariant", "FechaCierreLocal"),
            new(
                "__RequestVerificationToken",
                ExtraerTokenAntiforgery(formulario))
        };
        using var datos = new FormUrlEncodedContent(valores);

        var respuesta = await cliente.PostAsync("/licitaciones/crear", datos);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var guardada = await contextoVerificacion.Licitaciones
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal("lic-2030-001", guardada.Codigo);
        Assert.Equal("LIC-2030-001", guardada.CodigoNormalizado);
        Assert.Equal("Compra de equipo informático", guardada.Titulo);
        Assert.Equal(1_250_000.50m, guardada.PresupuestoEstimadoCrc);
        Assert.Equal(
            new DateTimeOffset(2030, 10, 16, 0, 30, 0, TimeSpan.Zero),
            guardada.FechaCierre);
        Assert.Equal(EstadoLicitacion.Borrador, guardada.Estado);
        Assert.NotEqual(0u, guardada.Version);
    }

    private static string ExtraerTokenAntiforgery(string contenido)
    {
        var coincidencia = PatronTokenAntiforgery.Match(contenido);
        Assert.True(
            coincidencia.Success,
            "No se encontró el token antiforgery.");

        return WebUtility.HtmlDecode(coincidencia.Groups[1].Value);
    }

    private sealed class WebFactory(
        string cadenaConexion) : WebApplicationFactory<WebAssemblyMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:Licitaciones",
                cadenaConexion);
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
                services
                    .AddDataProtection()
                    .UseEphemeralDataProtectionProvider());
        }
    }
}
