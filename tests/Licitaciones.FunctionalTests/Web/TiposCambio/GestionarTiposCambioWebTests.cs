using System.Net;
using System.Text.RegularExpressions;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.TiposCambio;
using Licitaciones.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Web.TiposCambio;

public sealed class GestionarTiposCambioWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    [Fact]
    public async Task Post_EditarTipoInactivo_ConActivoExistente_MuestraValidacionSin500()
    {
        var repositorio = new RepositorioTiposCambioEnMemoria();
        var actual = repositorio.Agregar(560m, true);
        var editar = repositorio.Agregar(500m, false);
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion);

        var pagina = await cliente.GetAsync($"/tipos-cambio/{editar.Id}/editar");
        pagina.EnsureSuccessStatusCode();
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["CrcPorUsd"] = "500",
            ["Activo"] = "true",
            ["__RequestVerificationToken"] = ExtraerToken(await pagina.Content.ReadAsStringAsync())
        });

        var respuesta = await cliente.PostAsync($"/tipos-cambio/{editar.Id}/editar", datos);
        var contenido = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Ya existe un tipo de cambio activo.", contenido);
        Assert.Equal("500.00", repositorio.Obtener(editar.Id).CrcPorUsd.ToString("0.00"));
        Assert.False(repositorio.Obtener(editar.Id).Activo);
        Assert.True(repositorio.Obtener(actual.Id).Activo);
        Assert.Single(repositorio.TiposCambio, x => x.Activo);
    }

    [Fact]
    public async Task Post_CrearTipoActivo_ConActivoExistente_MuestraValidacionSin500()
    {
        var repositorio = new RepositorioTiposCambioEnMemoria();
        var actual = repositorio.Agregar(560m, true);
        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion);

        var pagina = await cliente.GetAsync("/tipos-cambio/crear");
        pagina.EnsureSuccessStatusCode();
        using var datos = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["CrcPorUsd"] = "500",
            ["Activo"] = "true",
            ["__RequestVerificationToken"] = ExtraerToken(await pagina.Content.ReadAsStringAsync())
        });

        var respuesta = await cliente.PostAsync("/tipos-cambio/crear", datos);
        var contenido = await respuesta.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Ya existe un tipo de cambio activo.", contenido);
        Assert.True(repositorio.Obtener(actual.Id).Activo);
        Assert.Single(repositorio.TiposCambio, x => x.Activo);
        Assert.DoesNotContain(repositorio.TiposCambio, x => x.CrcPorUsd == 500m);
    }

    private static string ExtraerToken(string contenido)
    {
        var coincidencia = PatronTokenAntiforgery.Match(contenido);
        Assert.True(coincidencia.Success, "No se encontró el token antiforgery.");
        return coincidencia.Groups[1].Value;
    }

    private static HttpClient CrearCliente(WebFactory aplicacion) =>
        aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private sealed class WebFactory(RepositorioTiposCambioEnMemoria repositorio)
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
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
                services.RemoveAll<ITipoCambioRepository>();
                services.RemoveAll<TipoCambioService>();
                services.AddSingleton<ITipoCambioRepository>(repositorio);
                services.AddScoped<TipoCambioService>();
            });
        }
    }

    private sealed class RepositorioTiposCambioEnMemoria : ITipoCambioRepository
    {
        public List<TipoCambio> TiposCambio { get; } = [];
        private TipoCambio? _activoAntesDeGuardar;
        private TipoCambio? _tipoEditado;
        private bool _seIntentoActivarConActivo;

        public TipoCambio Agregar(decimal valor, bool activo)
        {
            var tipo = new TipoCambio(valor, DateTimeOffset.UtcNow);
            if (activo) tipo.Activar(DateTimeOffset.UtcNow);
            TiposCambio.Add(tipo);
            return tipo;
        }

        public Task AgregarAsync(TipoCambio tipo, CancellationToken cancellationToken = default)
        {
            if (_seIntentoActivarConActivo)
            {
                _activoAntesDeGuardar?.Activar(DateTimeOffset.UtcNow);
                throw new InvalidOperationException("Solo puede existir un tipo de cambio activo.");
            }

            TiposCambio.Add(tipo);
            return Task.CompletedTask;
        }

        public Task<TipoCambio?> ObtenerAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tipo = TiposCambio.SingleOrDefault(x => x.Id == id);
            _tipoEditado = tipo;
            _activoAntesDeGuardar = TiposCambio.SingleOrDefault(x => x.Activo);
            return Task.FromResult(tipo);
        }

        public Task<IReadOnlyList<TipoCambio>> ListarAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TipoCambio>>(TiposCambio);

        public Task<TipoCambio?> ObtenerActivoAsync(CancellationToken cancellationToken = default) =>
            ObtenerActivoAsyncCore();

        private Task<TipoCambio?> ObtenerActivoAsyncCore()
        {
            var activo = TiposCambio.SingleOrDefault(x => x.Activo);
            _activoAntesDeGuardar ??= activo;
            _seIntentoActivarConActivo |= activo is not null;
            return Task.FromResult(activo);
        }

        public Task GuardarAsync(CancellationToken cancellationToken = default)
        {
            foreach (var tipo in TiposCambio.Where(x => x != _activoAntesDeGuardar && x.Activo))
            {
                tipo.Desactivar(DateTimeOffset.UtcNow);
            }

            _activoAntesDeGuardar?.Activar(DateTimeOffset.UtcNow);
            _tipoEditado?.Desactivar(DateTimeOffset.UtcNow);
            throw new InvalidOperationException("Solo puede existir un tipo de cambio activo.");
        }

        public Task EliminarAsync(TipoCambio tipo, CancellationToken cancellationToken = default)
        {
            TiposCambio.Remove(tipo);
            return Task.CompletedTask;
        }

        public TipoCambio Obtener(Guid id) => TiposCambio.Single(x => x.Id == id);
    }
}
