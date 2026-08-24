using System.Net;
using System.Text.RegularExpressions;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Web.Proveedores;

public sealed class RegistrarProveedorWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    [Fact]
    public async Task Post_ConInfraestructuraReal_PersisteProveedor()
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
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

        var paginaFormulario = await cliente.GetAsync("/proveedores/registrar");
        paginaFormulario.EnsureSuccessStatusCode();
        var formulario = await paginaFormulario.Content.ReadAsStringAsync();
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Nombre"] = "  Empresa   Central  ",
            ["__RequestVerificationToken"] = ExtraerTokenAntiforgery(formulario)
        });

        var respuesta = await cliente.PostAsync("/proveedores/registrar", datos);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var proveedorGuardado = await contextoVerificacion.Proveedores
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal("Empresa Central", proveedorGuardado.Nombre);
        Assert.Equal("EMPRESA CENTRAL", proveedorGuardado.NombreNormalizado);
    }

    private static string ExtraerTokenAntiforgery(string contenido)
    {
        var coincidencia = PatronTokenAntiforgery.Match(contenido);
        Assert.True(coincidencia.Success, "No se encontró el token antiforgery.");

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
