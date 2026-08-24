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

public sealed class PublicarLicitacionWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    [Fact]
    public async Task Post_ConInfraestructuraReal_PublicaBorradorYConfirmaConPrg()
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
        var fechaCierre = DateTimeOffset.FromUnixTimeSeconds(
            DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds());
        var licitacion = new Licitacion(
            "LIC-2030-WEB-001",
            "Compra web integrada",
            3_500_000m,
            fechaCierre,
            DateTimeOffset.UtcNow.AddDays(-1));

        await using (var contexto = new LicitacionesDbContext(opciones))
        {
            await contexto.Database.MigrateAsync();
            await contexto.Licitaciones.AddAsync(licitacion);
            await contexto.SaveChangesAsync();
        }

        var updatedAtInicial = licitacion.UpdatedAt;
        var versionInicial = licitacion.Version;
        var rutaPublicacion = $"/licitaciones/{licitacion.Id}/publicar";

        await using var aplicacion = new WebFactory(
            postgres.GetConnectionString());
        using var cliente = aplicacion.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost"),
                HandleCookies = true
            });

        var paginaConfirmacion = await cliente.GetAsync(rutaPublicacion);
        paginaConfirmacion.EnsureSuccessStatusCode();
        var formulario = await paginaConfirmacion.Content.ReadAsStringAsync();
        Assert.Contains($"action=\"{rutaPublicacion}\"", formulario);
        var token = ExtraerTokenAntiforgery(formulario);
        using var datos = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            });
        var instanteAnterior = DateTimeOffset.UtcNow;

        var respuesta = await cliente.PostAsync(rutaPublicacion, datos);

        var instantePosterior = DateTimeOffset.UtcNow;
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal(
            rutaPublicacion,
            respuesta.Headers.Location?.OriginalString);

        var confirmacion = await cliente.GetAsync(respuesta.Headers.Location);
        confirmacion.EnsureSuccessStatusCode();
        var contenido = WebUtility.HtmlDecode(
            await confirmacion.Content.ReadAsStringAsync());
        Assert.Contains("Licitación publicada correctamente.", contenido);
        Assert.Contains("<strong>Publicada</strong>", contenido);

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var guardada = await contextoVerificacion.Licitaciones
            .AsNoTracking()
            .SingleAsync(actual => actual.Id == licitacion.Id);

        Assert.Equal(EstadoLicitacion.Publicada, guardada.Estado);
        Assert.NotEqual(updatedAtInicial, guardada.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, guardada.UpdatedAt.Offset);
        Assert.InRange(
            guardada.UpdatedAt,
            instanteAnterior.AddMilliseconds(-1),
            instantePosterior);
        Assert.NotEqual(versionInicial, guardada.Version);
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
