using System.Net;
using System.Text.RegularExpressions;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Web.Proveedores;

/// <summary>
/// Pruebas de regresion para el CrudController de proveedores.
/// Bug original: GestionarProveedoresService no estaba registrado en el DI de Program.cs,
/// causando HTTP 500 al intentar acceder a /gestion/proveedores/{id} o /gestion/proveedores/{id}/editar.
/// </summary>
public sealed class GestionarProveedoresWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    [Fact]
    public async Task Get_ListarProveedores_RetornaOkConTabla()
    {
        // Arrange
        var repositorio = new RepositorioProveedoresEnMemoria();
        repositorio.Proveedores.Add(new Proveedor("Empresa Alfa"));
        repositorio.Proveedores.Add(new Proveedor("Empresa Beta"));
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion);

        // Act
        var respuesta = await cliente.GetAsync("/gestion/proveedores");

        // Assert
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Empresa Alfa", contenido);
        Assert.Contains("Empresa Beta", contenido);
    }

    [Fact]
    public async Task Get_VerProveedor_RetornaOkConDetalle()
    {
        // Arrange - regresion: antes fallaba con HTTP 500 porque GestionarProveedoresService
        // no estaba registrado en el contenedor DI de Program.cs.
        var repositorio = new RepositorioProveedoresEnMemoria();
        var proveedor = new Proveedor("Constructora Sur");
        repositorio.Proveedores.Add(proveedor);
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion);

        // Act
        var respuesta = await cliente.GetAsync($"/gestion/proveedores/{proveedor.Id}");

        // Assert
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Constructora Sur", contenido);
        Assert.Contains("Detalle del proveedor", contenido);
    }

    [Fact]
    public async Task Get_EditarProveedor_RetornaOkConFormulario()
    {
        // Arrange - regresion: antes fallaba con HTTP 500 porque GestionarProveedoresService
        // no estaba registrado en el contenedor DI de Program.cs.
        var repositorio = new RepositorioProveedoresEnMemoria();
        var proveedor = new Proveedor("Distribuidora Norte");
        repositorio.Proveedores.Add(proveedor);
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion);

        // Act
        var respuesta = await cliente.GetAsync($"/gestion/proveedores/{proveedor.Id}/editar");

        // Assert
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Distribuidora Norte", contenido);
        Assert.Contains("Editar proveedor", contenido);
        Assert.Contains("__RequestVerificationToken", contenido);
    }

    [Fact]
    public async Task Post_EditarProveedor_ConNombreValido_RedirigeAlDetalle()
    {
        // Arrange
        var repositorio = new RepositorioProveedoresEnMemoria();
        var proveedor = new Proveedor("Nombre Original");
        repositorio.Proveedores.Add(proveedor);
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

        var paginaEditar = await cliente.GetAsync($"/gestion/proveedores/{proveedor.Id}/editar");
        paginaEditar.EnsureSuccessStatusCode();
        var formulario = await paginaEditar.Content.ReadAsStringAsync();
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = proveedor.Id.ToString(),
            ["Nombre"] = "Nombre Actualizado",
            ["__RequestVerificationToken"] = ExtraerTokenAntiforgery(formulario)
        });

        // Act
        var respuesta = await cliente.PostAsync($"/gestion/proveedores/{proveedor.Id}/editar", datos);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal(
            $"/gestion/proveedores/{proveedor.Id}",
            respuesta.Headers.Location?.OriginalString);
        Assert.Equal("Nombre Actualizado", repositorio.Proveedores[0].Nombre);
    }

    [Fact]
    public async Task Post_EditarProveedor_ConNombreDuplicado_MuestraErrorSin500()
    {
        // Arrange
        var repositorio = new RepositorioProveedoresEnMemoria();
        repositorio.Proveedores.Add(new Proveedor("Empresa Existente"));
        var proveedor = new Proveedor("Empresa A Editar");
        repositorio.Proveedores.Add(proveedor);
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

        var paginaEditar = await cliente.GetAsync($"/gestion/proveedores/{proveedor.Id}/editar");
        paginaEditar.EnsureSuccessStatusCode();
        var formulario = await paginaEditar.Content.ReadAsStringAsync();
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = proveedor.Id.ToString(),
            ["Nombre"] = "Empresa Existente",
            ["__RequestVerificationToken"] = ExtraerTokenAntiforgery(formulario)
        });

        // Act
        var respuesta = await cliente.PostAsync($"/gestion/proveedores/{proveedor.Id}/editar", datos);

        // Assert
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("Ya existe un proveedor con el mismo nombre.", contenido);
        Assert.Equal("Empresa A Editar", repositorio.Proveedores.Single(p => p.Id == proveedor.Id).Nombre);
        Assert.Equal(2, repositorio.Proveedores.Count);
        Assert.DoesNotContain(
            repositorio.Proveedores,
            p => p.Nombre == "Empresa Existente" && p.Id == proveedor.Id);
    }

    [Fact]
    public async Task Post_EliminarProveedor_SinOfertas_RedirigeAlListado()
    {
        // Arrange
        var repositorio = new RepositorioProveedoresEnMemoria();
        var proveedor = new Proveedor("Proveedor Eliminable");
        repositorio.Proveedores.Add(proveedor);
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

        var paginaDetalle = await cliente.GetAsync($"/gestion/proveedores/{proveedor.Id}");
        paginaDetalle.EnsureSuccessStatusCode();
        var html = await paginaDetalle.Content.ReadAsStringAsync();
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = ExtraerTokenAntiforgery(html)
        });

        // Act
        var respuesta = await cliente.PostAsync($"/gestion/proveedores/{proveedor.Id}/eliminar", datos);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/gestion/proveedores", respuesta.Headers.Location?.OriginalString);
        Assert.Empty(repositorio.Proveedores);
    }

    [Fact]
    public async Task Post_EliminarProveedor_ConOfertas_MuestraMensajeErrorSin500()
    {
        // Arrange
        var repositorio = new RepositorioProveedoresEnMemoria();
        var proveedor = new Proveedor("Proveedor Con Ofertas");
        repositorio.Proveedores.Add(proveedor);
        repositorio.ProveedorConOfertas = proveedor.Id;
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });

        var paginaDetalle = await cliente.GetAsync($"/gestion/proveedores/{proveedor.Id}");
        paginaDetalle.EnsureSuccessStatusCode();
        var html = await paginaDetalle.Content.ReadAsStringAsync();
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = ExtraerTokenAntiforgery(html)
        });

        // Act
        var respuesta = await cliente.PostAsync($"/gestion/proveedores/{proveedor.Id}/eliminar", datos);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal(
            $"/gestion/proveedores/{proveedor.Id}",
            respuesta.Headers.Location?.OriginalString);
        Assert.Single(repositorio.Proveedores);
    }

    private static string ExtraerTokenAntiforgery(string contenido)
    {
        var coincidencia = PatronTokenAntiforgery.Match(contenido);
        Assert.True(coincidencia.Success, "No se encontro el token antiforgery.");
        return WebUtility.HtmlDecode(coincidencia.Groups[1].Value);
    }

    private static HttpClient CrearCliente(WebFactory aplicacion) =>
        aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private sealed class WebFactory(
        RepositorioProveedoresEnMemoria? repositorio = null)
        : WebApplicationFactory<WebAssemblyMarker>
    {
        private readonly RepositorioProveedoresEnMemoria _repositorio =
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
                services.RemoveAll<IOfertaRepository>();
                services.AddSingleton<IProveedorRepository>(_repositorio);
                services.AddSingleton<IOfertaRepository>(_repositorio);
                services.RemoveAll<RegistrarProveedorService>();
                services.RemoveAll<GestionarProveedoresService>();
                services.AddScoped<RegistrarProveedorService>();
                services.AddScoped<GestionarProveedoresService>();
            });
        }
    }

    private sealed class RepositorioProveedoresEnMemoria
        : IProveedorRepository, IOfertaRepository
    {
        private readonly Dictionary<Guid, (string Nombre, string NombreNormalizado, DateTimeOffset UpdatedAt)> _estadosPersistidos = [];

        public List<Proveedor> Proveedores { get; } = [];

        public Guid? ProveedorConOfertas { get; set; }

        // --- IProveedorRepository ---

        public Task<IReadOnlyList<Proveedor>> ListarAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Proveedor>>(Proveedores);
        }

        public Task<bool> ExisteConNombreNormalizadoAsync(
            string nombreNormalizado,
            CancellationToken cancellationToken = default)
        {
            var existe = Proveedores.Any(p => p.NombreNormalizado == nombreNormalizado);
            return Task.FromResult(existe);
        }

        public Task AgregarAsync(
            Proveedor proveedor,
            CancellationToken cancellationToken = default)
        {
            Proveedores.Add(proveedor);
            return Task.CompletedTask;
        }

        public Task<Proveedor?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var proveedor = Proveedores.SingleOrDefault(p => p.Id == id);
            if (proveedor is not null)
            {
                _estadosPersistidos[id] = (proveedor.Nombre, proveedor.NombreNormalizado, proveedor.UpdatedAt);
            }

            return Task.FromResult(proveedor);
        }

        public Task EliminarAsync(
            Proveedor proveedor,
            CancellationToken cancellationToken = default)
        {
            Proveedores.Remove(proveedor);
            return Task.CompletedTask;
        }

        public Task GuardarCambiosAsync(
            Proveedor proveedor,
            CancellationToken cancellationToken = default)
        {
            if (Proveedores.Any(p => p.Id != proveedor.Id &&
                                     p.NombreNormalizado == proveedor.NombreNormalizado))
            {
                var estadoPersistido = _estadosPersistidos[proveedor.Id];
                typeof(Proveedor).GetProperty(nameof(Proveedor.Nombre))!
                    .SetValue(proveedor, estadoPersistido.Nombre);
                typeof(Proveedor).GetProperty(nameof(Proveedor.NombreNormalizado))!
                    .SetValue(proveedor, estadoPersistido.NombreNormalizado);
                typeof(Proveedor).GetProperty(nameof(Proveedor.UpdatedAt))!
                    .SetValue(proveedor, estadoPersistido.UpdatedAt);
                throw new ProveedorDuplicadoException();
            }

            return Task.CompletedTask;
        }

        // --- IOfertaRepository ---

        public Task AgregarAsync(Oferta oferta, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<Oferta?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<Oferta?>(null);

        public Task<IReadOnlyList<Oferta>> ListarAsync(
            Guid? licitacionId,
            Guid? proveedorId,
            CancellationToken ct = default)
        {
            IReadOnlyList<Oferta> resultado =
                (ProveedorConOfertas.HasValue && proveedorId == ProveedorConOfertas)
                    ? [new Oferta(Guid.NewGuid(), ProveedorConOfertas.Value, 1_000m, DateTimeOffset.UtcNow)]
                    : [];
            return Task.FromResult(resultado);
        }

        public Task<bool> ExisteParaParejaAsync(
            Guid licitacionId,
            Guid proveedorId,
            Guid? excluirId = null,
            CancellationToken ct = default) =>
            Task.FromResult(false);

        public Task GuardarAsync(CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task EliminarAsync(Oferta oferta, CancellationToken ct = default) =>
            Task.CompletedTask;
    }
}