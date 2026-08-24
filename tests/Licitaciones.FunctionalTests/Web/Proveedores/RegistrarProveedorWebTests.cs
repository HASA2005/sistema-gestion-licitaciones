using System.Net;
using System.Text.RegularExpressions;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Web.Proveedores;

public sealed class RegistrarProveedorWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    [Fact]
    public async Task Get_Registrar_MuestraFormularioConProteccionAntiforgery()
    {
        await using var aplicacion = new WebFactory();
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var respuesta = await cliente.GetAsync("/proveedores/registrar");

        var contenido = await respuesta.Content.ReadAsStringAsync();

        Assert.True(
            respuesta.StatusCode == HttpStatusCode.OK,
            $"Se esperaba 200 OK, pero se obtuvo {(int)respuesta.StatusCode}. Cuerpo: {contenido}");
        Assert.Equal(
            "text/html",
            respuesta.Content.Headers.ContentType?.MediaType);

        Assert.Contains("Registrar proveedor", contenido);
        Assert.Contains("name=\"Nombre\"", contenido);
        Assert.Contains("__RequestVerificationToken", contenido);
    }

    [Fact]
    public async Task Get_Inicio_OfreceAccesoAlRegistroDeProveedores()
    {
        await using var aplicacion = new WebFactory();
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var respuesta = await cliente.GetAsync("/");

        respuesta.EnsureSuccessStatusCode();
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("<html lang=\"es\">", contenido);
        Assert.Contains("href=\"/proveedores/registrar\"", contenido);
        Assert.Contains("Registrar proveedor", contenido);
    }

    [Fact]
    public async Task Post_ConNombreValido_GuardaYRedirigeConConfirmacion()
    {
        var repositorio = new RepositorioProveedoresEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
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
        Assert.Equal(
            "/proveedores/registrar",
            respuesta.Headers.Location?.OriginalString);

        var proveedorGuardado = Assert.Single(repositorio.Proveedores);
        Assert.Equal("Empresa Central", proveedorGuardado.Nombre);
        Assert.Equal("EMPRESA CENTRAL", proveedorGuardado.NombreNormalizado);

        var confirmacion = await cliente.GetAsync(respuesta.Headers.Location);
        confirmacion.EnsureSuccessStatusCode();
        var contenido = await confirmacion.Content.ReadAsStringAsync();
        Assert.Contains("Proveedor registrado correctamente.", contenido);
    }

    [Fact]
    public async Task Post_SinTokenAntiforgery_DevuelveBadRequestYNoGuarda()
    {
        var repositorio = new RepositorioProveedoresEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Nombre"] = "Empresa Central"
        });

        var respuesta = await cliente.PostAsync("/proveedores/registrar", datos);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Empty(repositorio.Proveedores);
    }

    [Theory]
    [InlineData("", "El nombre del proveedor es obligatorio.")]
    [InlineData("   ", "El nombre del proveedor es obligatorio.")]
    [InlineData(
        "Empresa @ Central",
        "El nombre del proveedor contiene caracteres no permitidos.")]
    public async Task Post_ConNombreInvalido_MuestraErrorYNoGuarda(
        string nombre,
        string mensajeEsperado)
    {
        var repositorio = new RepositorioProveedoresEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
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
            ["Nombre"] = nombre,
            ["__RequestVerificationToken"] = ExtraerTokenAntiforgery(formulario)
        });

        var respuesta = await cliente.PostAsync("/proveedores/registrar", datos);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains(mensajeEsperado, contenido);
        Assert.Contains($"value=\"{WebUtility.HtmlEncode(nombre)}\"", contenido);
        Assert.Empty(repositorio.Proveedores);
    }

    [Fact]
    public async Task Post_ConNombreDuplicado_MuestraErrorYNoDuplica()
    {
        var repositorio = new RepositorioProveedoresEnMemoria();
        repositorio.Proveedores.Add(new Proveedor("Empresa Central"));

        await using var aplicacion = new WebFactory(repositorio);
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
            ["Nombre"] = " empresa   central ",
            ["__RequestVerificationToken"] = ExtraerTokenAntiforgery(formulario)
        });

        var respuesta = await cliente.PostAsync("/proveedores/registrar", datos);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("Ya existe un proveedor con el mismo nombre.", contenido);
        Assert.Contains("value=\" empresa   central \"", contenido);
        Assert.Single(repositorio.Proveedores);
    }

    private static string ExtraerTokenAntiforgery(string contenido)
    {
        var coincidencia = PatronTokenAntiforgery.Match(contenido);
        Assert.True(coincidencia.Success, "No se encontró el token antiforgery.");

        return WebUtility.HtmlDecode(coincidencia.Groups[1].Value);
    }

    private sealed class WebFactory(
        IProveedorRepository? repositorio = null)
        : WebApplicationFactory<WebAssemblyMarker>
    {
        private readonly IProveedorRepository _repositorio =
            repositorio ?? new RepositorioProveedoresEnMemoria();

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
                services.RemoveAll<IProveedorRepository>();
                services.RemoveAll<RegistrarProveedorService>();
                services.AddSingleton(_repositorio);
                services.AddScoped<RegistrarProveedorService>();
            });
        }
    }

    private sealed class RepositorioProveedoresEnMemoria
        : IProveedorRepository
    {
        public List<Proveedor> Proveedores { get; } = [];

        public Task<bool> ExisteConNombreNormalizadoAsync(
            string nombreNormalizado,
            CancellationToken cancellationToken = default)
        {
            var existe = Proveedores.Any(
                proveedor => proveedor.NombreNormalizado == nombreNormalizado);

            return Task.FromResult(existe);
        }

        public Task AgregarAsync(
            Proveedor proveedor,
            CancellationToken cancellationToken = default)
        {
            Proveedores.Add(proveedor);
            return Task.CompletedTask;
        }
    }
}
