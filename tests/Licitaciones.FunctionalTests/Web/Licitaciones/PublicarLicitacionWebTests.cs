using System.Net;
using System.Text.RegularExpressions;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Web.Licitaciones;

public sealed class PublicarLicitacionWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    [Fact]
    public async Task Get_ConBorrador_MuestraDatosFormularioYAntiforgery()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(5));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion);

        var respuesta = await cliente.GetAsync(
            $"/licitaciones/{licitacion.Id}/publicar");

        respuesta.EnsureSuccessStatusCode();
        var contenido = WebUtility.HtmlDecode(
            await respuesta.Content.ReadAsStringAsync());
        Assert.Contains("Publicar licitación", contenido);
        Assert.Contains("LIC-2030-001", contenido);
        Assert.Contains("Compra de equipo", contenido);
        Assert.Contains("Borrador", contenido);
        Assert.Contains(
            $"action=\"/licitaciones/{licitacion.Id}/publicar\"",
            contenido);
        Assert.Contains("method=\"post\"", contenido);
        Assert.Contains("__RequestVerificationToken", contenido);
    }

    [Fact]
    public async Task Post_ConBorradorValido_PublicaAplicaPrgYOcultaBoton()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(5));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var token = await ObtenerTokenAntiforgeryAsync(cliente, licitacion.Id);
        using var formulario = CrearFormulario(token);

        var respuesta = await cliente.PostAsync(
            $"/licitaciones/{licitacion.Id}/publicar",
            formulario);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal(
            $"/licitaciones/{licitacion.Id}/publicar",
            respuesta.Headers.Location?.OriginalString);
        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.Equal(1, repositorio.CantidadGuardados);

        var confirmacion = await cliente.GetAsync(respuesta.Headers.Location);
        confirmacion.EnsureSuccessStatusCode();
        var contenido = WebUtility.HtmlDecode(
            await confirmacion.Content.ReadAsStringAsync());
        Assert.Contains("Licitación publicada correctamente.", contenido);
        Assert.Contains("Publicada", contenido);
        Assert.DoesNotContain(
            $"action=\"/licitaciones/{licitacion.Id}/publicar\"",
            contenido);
    }

    [Fact]
    public async Task Get_ConIdInexistente_DevuelveNotFound()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion);

        var respuesta = await cliente.GetAsync(
            $"/licitaciones/{Guid.NewGuid()}/publicar");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Post_ConFechaVencida_MuestraErrorYNoGuarda()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(-1));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var token = await ObtenerTokenAntiforgeryAsync(cliente, licitacion.Id);
        using var formulario = CrearFormulario(token);

        var respuesta = await cliente.PostAsync(
            $"/licitaciones/{licitacion.Id}/publicar",
            formulario);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal(0, repositorio.CantidadGuardados);

        var confirmacion = await cliente.GetAsync(respuesta.Headers.Location);
        confirmacion.EnsureSuccessStatusCode();
        var contenido = WebUtility.HtmlDecode(
            await confirmacion.Content.ReadAsStringAsync());
        Assert.Contains(
            "La fecha de cierre debe ser futura para publicar la licitación.",
            contenido);
    }

    [Fact]
    public async Task Post_ConConflicto_MuestraMensajeSeguro()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(5));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion)
        {
            SimularConflicto = true
        };

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var token = await ObtenerTokenAntiforgeryAsync(cliente, licitacion.Id);
        using var formulario = CrearFormulario(token);

        var respuesta = await cliente.PostAsync(
            $"/licitaciones/{licitacion.Id}/publicar",
            formulario);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        var confirmacion = await cliente.GetAsync(respuesta.Headers.Location);
        confirmacion.EnsureSuccessStatusCode();
        var contenido = WebUtility.HtmlDecode(
            await confirmacion.Content.ReadAsStringAsync());
        Assert.Contains(
            "La licitación fue modificada por otra operación. Actualice los datos e intente nuevamente.",
            contenido);
        Assert.DoesNotContain("DbUpdate", contenido);
        Assert.DoesNotContain("xmin", contenido);
    }

    [Fact]
    public async Task Post_SinAntiforgery_DevuelveBadRequestYNoPublica()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(5));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        using var formulario = new FormUrlEncodedContent([]);

        var respuesta = await cliente.PostAsync(
            $"/licitaciones/{licitacion.Id}/publicar",
            formulario);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    private static Licitacion CrearLicitacion(DateTimeOffset fechaCierre)
    {
        return new Licitacion(
            "LIC-2030-001",
            "Compra de equipo",
            1_250_000m,
            fechaCierre,
            DateTimeOffset.UtcNow.AddDays(-1));
    }

    private static HttpClient CrearCliente(
        WebFactory aplicacion,
        bool permitirRedireccion = true)
    {
        return aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = permitirRedireccion,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });
    }

    private static async Task<string> ObtenerTokenAntiforgeryAsync(
        HttpClient cliente,
        Guid id)
    {
        var respuesta = await cliente.GetAsync(
            $"/licitaciones/{id}/publicar");
        respuesta.EnsureSuccessStatusCode();
        var contenido = await respuesta.Content.ReadAsStringAsync();
        var coincidencia = PatronTokenAntiforgery.Match(contenido);
        Assert.True(coincidencia.Success, "No se encontró el token antiforgery.");

        return WebUtility.HtmlDecode(coincidencia.Groups[1].Value);
    }

    private static FormUrlEncodedContent CrearFormulario(string token)
    {
        return new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            });
    }

    private sealed class WebFactory(
        ILicitacionRepository repositorio)
        : WebApplicationFactory<WebAssemblyMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:Licitaciones",
                "Host=localhost;Database=licitaciones_tests;Username=test;Password=test");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                services
                    .AddDataProtection()
                    .UseEphemeralDataProtectionProvider();
                services.RemoveAll<ILicitacionRepository>();
                services.AddSingleton(repositorio);
            });
        }
    }

    private sealed class RepositorioLicitacionesEnMemoria(
        Licitacion? licitacion = null) : ILicitacionRepository
    {
        public bool SimularConflicto { get; init; }

        public int CantidadGuardados { get; private set; }

        public Task<bool> ExisteConCodigoNormalizadoAsync(
            string codigoNormalizado,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task AgregarAsync(
            Licitacion nuevaLicitacion,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Licitacion?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                licitacion?.Id == id ? licitacion : null);
        }

        public Task GuardarCambiosAsync(
            Licitacion licitacionModificada,
            CancellationToken cancellationToken = default)
        {
            CantidadGuardados++;
            if (SimularConflicto)
            {
                throw new LicitacionConcurrenciaException();
            }

            return Task.CompletedTask;
        }
    }
}
