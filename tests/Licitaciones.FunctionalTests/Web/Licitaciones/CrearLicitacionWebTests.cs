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

public sealed class CrearLicitacionWebTests
{
    private static readonly Regex PatronTokenAntiforgery = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    public static TheoryData<string, string, string> TextosFueraDeLongitud =>
        new()
        {
            {
                "Codigo",
                new string('C', Licitacion.LongitudMaximaCodigo + 1),
                "El código de la licitación no puede superar los 100 caracteres."
            },
            {
                "Titulo",
                new string('T', Licitacion.LongitudMaximaTitulo + 1),
                "El título de la licitación no puede superar los 200 caracteres."
            }
        };

    [Fact]
    public async Task Get_Crear_MuestraFormularioAccesibleConAntiforgery()
    {
        await using var aplicacion = new WebFactory();
        using var cliente = CrearCliente(aplicacion);

        var respuesta = await cliente.GetAsync("/licitaciones/crear");

        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("text/html", respuesta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Crear licitación", contenido);
        Assert.Contains("estado <strong>Borrador</strong>", contenido);
        Assert.Contains("action=\"/licitaciones/crear\"", contenido);
        Assert.Contains("method=\"post\"", contenido);
        Assert.Contains("name=\"Codigo\"", contenido);
        Assert.Contains("maxlength=\"100\"", contenido);
        Assert.Contains("name=\"Titulo\"", contenido);
        Assert.Contains("maxlength=\"200\"", contenido);
        Assert.Contains("name=\"PresupuestoEstimadoCrc\"", contenido);
        Assert.Contains("type=\"number\"", contenido);
        Assert.Contains("step=\"0.01\"", contenido);
        Assert.Contains("name=\"FechaCierreLocal\"", contenido);
        Assert.Contains("type=\"datetime-local\"", contenido);
        Assert.Contains("aria-required=\"true\"", contenido);
        Assert.Contains("id=\"codigo-error\"", contenido);
        Assert.Contains("id=\"titulo-error\"", contenido);
        Assert.Contains("id=\"presupuesto-error\"", contenido);
        Assert.Contains("id=\"fecha-cierre-error\"", contenido);
        Assert.Contains("__RequestVerificationToken", contenido);
    }

    [Fact]
    public async Task Get_Inicio_OfreceAccesoACrearLicitacion()
    {
        await using var aplicacion = new WebFactory();
        using var cliente = CrearCliente(aplicacion);

        var respuesta = await cliente.GetAsync("/");

        respuesta.EnsureSuccessStatusCode();
        var contenido = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("href=\"/licitaciones/crear\"", contenido);
        Assert.Contains("Crear licitación", contenido);
    }

    [Fact]
    public async Task Post_ConDatosValidos_GuardaBorradorEnUtcYAplicaPrg()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var token = await ObtenerTokenAntiforgeryAsync(cliente);
        using var datos = CrearDatosValidos(token);

        var respuesta = await cliente.PostAsync("/licitaciones/crear", datos);

        var contenidoPost = await respuesta.Content.ReadAsStringAsync();
        Assert.True(
            respuesta.StatusCode == HttpStatusCode.Redirect,
            $"Se esperaba redirección, pero se obtuvo {(int)respuesta.StatusCode}. Cuerpo: {contenidoPost}");
        Assert.Equal(
            "/licitaciones/crear",
            respuesta.Headers.Location?.OriginalString);

        var guardada = Assert.Single(repositorio.Licitaciones);
        Assert.Equal("LIC-2026-017", guardada.Codigo);
        Assert.Equal("LIC-2026-017", guardada.CodigoNormalizado);
        Assert.Equal("Compra de equipo", guardada.Titulo);
        Assert.Equal(1_500_000.50m, guardada.PresupuestoEstimadoCrc);
        Assert.Equal(
            new DateTimeOffset(2030, 6, 15, 16, 30, 0, TimeSpan.Zero),
            guardada.FechaCierre);
        Assert.Equal(TimeSpan.Zero, guardada.FechaCierre.Offset);
        Assert.Equal(EstadoLicitacion.Borrador, guardada.Estado);

        var confirmacion = await cliente.GetAsync(respuesta.Headers.Location);
        confirmacion.EnsureSuccessStatusCode();
        var contenido = WebUtility.HtmlDecode(
            await confirmacion.Content.ReadAsStringAsync());
        Assert.Contains("Licitación creada correctamente.", contenido);
    }

    [Theory]
    [InlineData("Codigo", "", "El código de la licitación es obligatorio.")]
    [InlineData("Titulo", "   ", "El título de la licitación es obligatorio.")]
    [InlineData("PresupuestoEstimadoCrc", "", "El presupuesto estimado es obligatorio.")]
    [InlineData(
        "PresupuestoEstimadoCrc",
        "0",
        "El presupuesto estimado debe ser mayor que cero y no superar el máximo permitido.")]
    [InlineData(
        "PresupuestoEstimadoCrc",
        "10000000000000000",
        "El presupuesto estimado debe ser mayor que cero y no superar el máximo permitido.")]
    [InlineData("FechaCierreLocal", "", "La fecha y hora de cierre es obligatoria.")]
    [MemberData(nameof(TextosFueraDeLongitud))]
    public async Task Post_ConCampoInvalido_MuestraErrorYNoGuarda(
        string campo,
        string valor,
        string mensajeEsperado)
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var token = await ObtenerTokenAntiforgeryAsync(cliente);
        var valores = CrearValoresValidos(token);
        valores[campo] = valor;
        using var datos = CrearContenidoFormulario(valores);

        var respuesta = await cliente.PostAsync("/licitaciones/crear", datos);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contenido = WebUtility.HtmlDecode(
            await respuesta.Content.ReadAsStringAsync());
        Assert.True(
            contenido.Contains(mensajeEsperado, StringComparison.Ordinal),
            $"No se encontró el mensaje '{mensajeEsperado}'. Cuerpo: {contenido}");
        if (campo != "Codigo")
        {
            Assert.Contains("value=\"  LIC-2026-017  \"", contenido);
        }

        if (campo != "Titulo")
        {
            Assert.Contains("value=\"Compra de equipo\"", contenido);
        }

        Assert.Empty(repositorio.Licitaciones);
    }

    [Fact]
    public async Task Post_ConPrecisionInvalida_AsociaErrorAlPresupuestoYConservaDatos()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var token = await ObtenerTokenAntiforgeryAsync(cliente);
        var valores = CrearValoresValidos(token);
        valores["PresupuestoEstimadoCrc"] = "1500000.505";
        using var datos = CrearContenidoFormulario(valores);

        var respuesta = await cliente.PostAsync("/licitaciones/crear", datos);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contenido = WebUtility.HtmlDecode(
            await respuesta.Content.ReadAsStringAsync());
        Assert.Contains(
            "El presupuesto estimado no puede tener más de dos decimales.",
            contenido);
        Assert.Matches(
            "id=\"presupuesto-error\"[^>]*>El presupuesto estimado no puede tener más de dos decimales\\.",
            contenido);
        Assert.Contains("value=\"  LIC-2026-017  \"", contenido);
        Assert.Contains("value=\"Compra de equipo\"", contenido);
        Assert.Contains("value=\"1500000.505\"", contenido);
        Assert.Contains("value=\"2030-06-15T10:30\"", contenido);
        Assert.Empty(repositorio.Licitaciones);
    }

    [Fact]
    public async Task Post_ConCodigoDuplicado_MuestraErrorJuntoAlCodigoYNoDuplica()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();
        repositorio.Licitaciones.Add(new Licitacion(
            "LIC-2026-017",
            "Licitación existente",
            100m,
            new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero)));

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var token = await ObtenerTokenAntiforgeryAsync(cliente);
        using var datos = CrearDatosValidos(token);

        var respuesta = await cliente.PostAsync("/licitaciones/crear", datos);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contenido = WebUtility.HtmlDecode(
            await respuesta.Content.ReadAsStringAsync());
        Assert.Contains(
            "Ya existe una licitación con el mismo código.",
            contenido);
        Assert.Matches(
            "id=\"codigo-error\"[^>]*>Ya existe una licitación con el mismo código\\.",
            contenido);
        Assert.Contains("value=\"  LIC-2026-017  \"", contenido);
        Assert.Single(repositorio.Licitaciones);
    }

    [Fact]
    public async Task Post_SinTokenAntiforgery_DevuelveBadRequestYNoGuarda()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new WebFactory(repositorio);
        using var cliente = CrearCliente(aplicacion, permitirRedireccion: false);
        var valores = CrearValoresValidos(token: null);
        using var datos = CrearContenidoFormulario(valores);

        var respuesta = await cliente.PostAsync("/licitaciones/crear", datos);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Empty(repositorio.Licitaciones);
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

    private static FormUrlEncodedContent CrearDatosValidos(string token)
    {
        return CrearContenidoFormulario(CrearValoresValidos(token));
    }

    private static FormUrlEncodedContent CrearContenidoFormulario(
        IReadOnlyDictionary<string, string> valores)
    {
        var pares = valores
            .Select(valor => new KeyValuePair<string, string>(
                valor.Key,
                valor.Value))
            .ToList();
        pares.Add(new KeyValuePair<string, string>(
            "__Invariant",
            "PresupuestoEstimadoCrc"));
        pares.Add(new KeyValuePair<string, string>(
            "__Invariant",
            "FechaCierreLocal"));

        return new FormUrlEncodedContent(pares);
    }

    private static Dictionary<string, string> CrearValoresValidos(string? token)
    {
        var valores = new Dictionary<string, string>
        {
            ["Codigo"] = "  LIC-2026-017  ",
            ["Titulo"] = "Compra de equipo",
            ["PresupuestoEstimadoCrc"] = "1500000.50",
            ["FechaCierreLocal"] = "2030-06-15T10:30"
        };

        if (token is not null)
        {
            valores["__RequestVerificationToken"] = token;
        }

        return valores;
    }

    private static async Task<string> ObtenerTokenAntiforgeryAsync(
        HttpClient cliente)
    {
        var paginaFormulario = await cliente.GetAsync("/licitaciones/crear");
        paginaFormulario.EnsureSuccessStatusCode();
        var formulario = await paginaFormulario.Content.ReadAsStringAsync();
        var coincidencia = PatronTokenAntiforgery.Match(formulario);
        Assert.True(coincidencia.Success, "No se encontró el token antiforgery.");

        return WebUtility.HtmlDecode(coincidencia.Groups[1].Value);
    }

    private sealed class WebFactory(
        ILicitacionRepository? repositorio = null)
        : WebApplicationFactory<WebAssemblyMarker>
    {
        private readonly ILicitacionRepository _repositorio =
            repositorio ?? new RepositorioLicitacionesEnMemoria();

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
                services.RemoveAll<CrearLicitacionService>();
                services.AddSingleton(_repositorio);
                services.AddScoped<CrearLicitacionService>();
            });
        }
    }

    private sealed class RepositorioLicitacionesEnMemoria
        : ILicitacionRepository
    {
        public List<Licitacion> Licitaciones { get; } = [];

        public Task<bool> ExisteConCodigoNormalizadoAsync(
            string codigoNormalizado,
            CancellationToken cancellationToken = default)
        {
            var existe = Licitaciones.Any(
                licitacion =>
                    licitacion.CodigoNormalizado == codigoNormalizado);

            return Task.FromResult(existe);
        }

        public Task AgregarAsync(
            Licitacion licitacion,
            CancellationToken cancellationToken = default)
        {
            Licitaciones.Add(licitacion);
            return Task.CompletedTask;
        }
    }
}
