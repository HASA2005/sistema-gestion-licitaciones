using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Api.Licitaciones;

public sealed class CrearLicitacionEndpointTests
{
    private static readonly DateTimeOffset FechaCierreValida =
        new(2030, 10, 15, 18, 30, 0, TimeSpan.FromHours(-6));

    public static TheoryData<string> TextosFueraDeContrato => new()
    {
        CrearJsonConTextos(
            new string('C', Licitacion.LongitudMaximaCodigo + 1),
            "Compra"),
        CrearJsonConTextos(
            "LIC-1",
            new string('T', Licitacion.LongitudMaximaTitulo + 1)),
        CrearJsonConTextos("LIC-\0-1", "Compra"),
        CrearJsonConTextos("LIC-1", "Compra\nreservada")
    };

    [Fact]
    public async Task Post_ConDatosValidos_GuardaBorradorYDevuelveCreated()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/licitaciones",
            new
            {
                codigo = "  lic-2030-001  ",
                titulo = "  Compra de equipo informático  ",
                presupuestoEstimadoCrc = 1_250_000.50m,
                fechaCierre = FechaCierreValida
            });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.Null(respuesta.Headers.Location);

        var contenido = await respuesta.Content
            .ReadFromJsonAsync<CrearLicitacionRespuesta>();
        Assert.NotNull(contenido);
        Assert.NotEqual(Guid.Empty, contenido.Id);
        Assert.Equal("lic-2030-001", contenido.Codigo);
        Assert.Equal("Compra de equipo informático", contenido.Titulo);
        Assert.Equal(1_250_000.50m, contenido.PresupuestoEstimadoCrc);
        Assert.Equal(FechaCierreValida.ToUniversalTime(), contenido.FechaCierre);
        Assert.Equal("Borrador", contenido.Estado);
        Assert.Equal("Licitación creada correctamente.", contenido.Mensaje);

        var licitacionGuardada = Assert.Single(repositorio.Licitaciones);
        Assert.Equal(contenido.Id, licitacionGuardada.Id);
        Assert.Equal("lic-2030-001", licitacionGuardada.Codigo);
        Assert.Equal("LIC-2030-001", licitacionGuardada.CodigoNormalizado);
        Assert.Equal(
            "Compra de equipo informático",
            licitacionGuardada.Titulo);
        Assert.Equal(
            1_250_000.50m,
            licitacionGuardada.PresupuestoEstimadoCrc);
        Assert.Equal(
            FechaCierreValida.ToUniversalTime(),
            licitacionGuardada.FechaCierre);
        Assert.Equal(EstadoLicitacion.Borrador, licitacionGuardada.Estado);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData(
        "{\"titulo\":\"Compra\",\"presupuestoEstimadoCrc\":1000," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"presupuestoEstimadoCrc\":1000," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"Compra\"," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":1000}")]
    [InlineData(
        "{\"codigo\":\"   \",\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":1000," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"   \"," +
        "\"presupuestoEstimadoCrc\":1000," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":0," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":1000.123," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [MemberData(nameof(TextosFueraDeContrato))]
    public async Task Post_ConDatosAusentesOInvalidos_DevuelveUnprocessable(
        string json)
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();
        using var solicitud = CrearContenidoJson(json);

        var respuesta = await cliente.PostAsync(
            "/api/v1/licitaciones",
            solicitud);

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            respuesta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal("Datos de la licitación inválidos.", problema.Title);
        Assert.Equal(422, problema.Status);
        Assert.False(string.IsNullOrWhiteSpace(problema.Detail));
        Assert.Equal("licitacion_datos_invalidos", problema.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(problema.CorrelationId));
        Assert.DoesNotContain("Parameter", problema.Detail);
        Assert.DoesNotContain("Parámetro", problema.Detail);
        Assert.Empty(repositorio.Licitaciones);
    }

    [Fact]
    public async Task Post_ConCodigoDuplicado_DevuelveConflictProblemDetails()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();
        repositorio.Licitaciones.Add(
            new Licitacion(
                "LIC-2030-001",
                "Compra original",
                500_000m,
                FechaCierreValida));

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/licitaciones",
            new
            {
                codigo = "  lic-2030-001  ",
                titulo = "Otra compra",
                presupuestoEstimadoCrc = 750_000m,
                fechaCierre = FechaCierreValida
            });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal(
            "Código de licitación duplicado.",
            problema.Title);
        Assert.Equal(409, problema.Status);
        Assert.Equal(
            "Ya existe una licitación con el mismo código.",
            problema.Detail);
        Assert.Equal("licitacion_codigo_duplicado", problema.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(problema.CorrelationId));
        Assert.Single(repositorio.Licitaciones);
    }

    [Theory]
    [InlineData(
        "{\"codigo\":[],\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":1000," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":true," +
        "\"fechaCierre\":\"2030-10-15T18:30:00-06:00\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":1000," +
        "\"fechaCierre\":\"fecha-invalida\"}")]
    [InlineData(
        "{\"codigo\":\"LIC-1\",\"titulo\":\"Compra\"," +
        "\"presupuestoEstimadoCrc\":1000," +
        "\"fechaCierre\":\"2030-10-15T18:30:00\"}")]
    public async Task Post_ConTiposJsonInvalidos_DevuelveBadRequest(
        string json)
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();
        using var solicitud = CrearContenidoJson(json);

        var respuesta = await cliente.PostAsync(
            "/api/v1/licitaciones",
            solicitud);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal("Solicitud JSON inválida.", problema.Title);
        Assert.Equal(400, problema.Status);
        Assert.Equal("solicitud_json_invalida", problema.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(problema.CorrelationId));
        Assert.Empty(repositorio.Licitaciones);
    }

    [Fact]
    public async Task Post_SinJson_DevuelveUnsupportedMediaTypeProblemDetails()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();
        using var solicitud = new StringContent(
            "codigo=LIC-1",
            Encoding.UTF8,
            "text/plain");

        var respuesta = await cliente.PostAsync(
            "/api/v1/licitaciones",
            solicitud);

        Assert.Equal(
            HttpStatusCode.UnsupportedMediaType,
            respuesta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal("Tipo de contenido no compatible.", problema.Title);
        Assert.Equal(415, problema.Status);
        Assert.Equal("tipo_contenido_no_compatible", problema.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(problema.CorrelationId));
        Assert.Empty(repositorio.Licitaciones);
    }

    [Fact]
    public async Task OpenApi_DocumentaCreacionContratoYRespuestasEsperadas()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.GetAsync("/openapi/v1.json");
        respuesta.EnsureSuccessStatusCode();

        await using var contenido = await respuesta.Content.ReadAsStreamAsync();
        using var documento = await JsonDocument.ParseAsync(contenido);
        var operacion = documento.RootElement
            .GetProperty("paths")
            .GetProperty("/api/v1/licitaciones")
            .GetProperty("post");
        var respuestas = operacion.GetProperty("responses");

        Assert.True(respuestas.TryGetProperty("201", out _));
        Assert.True(respuestas.TryGetProperty("400", out _));
        Assert.True(respuestas.TryGetProperty("415", out _));
        Assert.True(respuestas.TryGetProperty("409", out _));
        Assert.True(respuestas.TryGetProperty("422", out _));
        Assert.True(respuestas.TryGetProperty("500", out _));

        var esquemaSolicitud = ResolverEsquema(
            documento.RootElement,
            operacion
                .GetProperty("requestBody")
                .GetProperty("content")
                .GetProperty("application/json")
                .GetProperty("schema"));
        var propiedades = esquemaSolicitud.GetProperty("properties");

        Assert.True(propiedades.TryGetProperty("codigo", out var codigo));
        Assert.Equal(100, codigo.GetProperty("maxLength").GetInt32());
        Assert.True(propiedades.TryGetProperty("titulo", out var titulo));
        Assert.Equal(200, titulo.GetProperty("maxLength").GetInt32());
        Assert.True(
            propiedades.TryGetProperty("presupuestoEstimadoCrc", out _));
        Assert.True(propiedades.TryGetProperty("fechaCierre", out _));
        Assert.False(propiedades.TryGetProperty("estado", out _));

        var propiedadesRequeridas = esquemaSolicitud
            .GetProperty("required")
            .EnumerateArray()
            .Select(propiedad => propiedad.GetString())
            .ToArray();
        Assert.Contains("codigo", propiedadesRequeridas);
        Assert.Contains("titulo", propiedadesRequeridas);
        Assert.Contains("presupuestoEstimadoCrc", propiedadesRequeridas);
        Assert.Contains("fechaCierre", propiedadesRequeridas);
    }

    private static StringContent CrearContenidoJson(string json)
    {
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static string CrearJsonConTextos(string codigo, string titulo)
    {
        return JsonSerializer.Serialize(new
        {
            codigo,
            titulo,
            presupuestoEstimadoCrc = 1_000m,
            fechaCierre = FechaCierreValida
        });
    }

    private static JsonElement ResolverEsquema(
        JsonElement documento,
        JsonElement esquema)
    {
        if (!esquema.TryGetProperty("$ref", out var referencia))
        {
            return esquema;
        }

        var nombreEsquema = referencia.GetString()!
            .Split('/')
            .Last();

        return documento
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(nombreEsquema);
    }

    private sealed record CrearLicitacionRespuesta(
        Guid Id,
        string Codigo,
        string Titulo,
        decimal PresupuestoEstimadoCrc,
        DateTimeOffset FechaCierre,
        string Estado,
        string Mensaje);

    private sealed record ProblemaRespuesta(
        string Title,
        int Status,
        string Detail,
        string ErrorCode,
        string CorrelationId);

    private sealed class ApiFactory(
        ILicitacionRepository repositorio) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:Licitaciones",
                "Host=localhost;Database=licitaciones_tests;" +
                "Username=test;Password=test");
            builder.ConfigureLogging(logging => logging.ClearProviders());

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ILicitacionRepository>();
                services.RemoveAll<CrearLicitacionService>();
                services.AddSingleton(repositorio);
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

        public Task<Licitacion?> ObtenerPorIdAsync(
            Guid licitacionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Licitaciones.SingleOrDefault(item => item.Id == licitacionId));
        }

        public Task GuardarCambiosAsync(
            Licitacion licitacion,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
