using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Api.Licitaciones;

public sealed class PublicarLicitacionEndpointTests
{
    [Fact]
    public async Task Post_ConBorradorValido_DevuelveOkYEstadoPublicado()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(5));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsync(
            $"/api/v1/licitaciones/{licitacion.Id}/publicar",
            content: null);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var contenido = await respuesta.Content
            .ReadFromJsonAsync<PublicarLicitacionRespuesta>();
        Assert.NotNull(contenido);
        Assert.Equal(licitacion.Id, contenido.Id);
        Assert.Equal("LIC-2030-001", contenido.Codigo);
        Assert.Equal("Compra de equipo", contenido.Titulo);
        Assert.Equal(1_250_000m, contenido.PresupuestoEstimadoCrc);
        Assert.Equal(licitacion.FechaCierre, contenido.FechaCierre);
        Assert.Equal("Publicada", contenido.Estado);
        Assert.Equal(TimeSpan.Zero, contenido.UpdatedAt.Offset);
        Assert.Equal(
            "Licitación publicada correctamente.",
            contenido.Mensaje);
        Assert.Equal(EstadoLicitacion.Publicada, licitacion.Estado);
        Assert.Equal(1, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Post_ConIdInvalido_DevuelveBadRequestProblemDetails()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsync(
            "/api/v1/licitaciones/no-es-uuid/publicar",
            content: null);

        await VerificarProblemaAsync(
            respuesta,
            HttpStatusCode.BadRequest,
            "identificador_licitacion_invalido");
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Post_ConIdInexistente_DevuelveNotFoundProblemDetails()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsync(
            $"/api/v1/licitaciones/{Guid.NewGuid()}/publicar",
            content: null);

        await VerificarProblemaAsync(
            respuesta,
            HttpStatusCode.NotFound,
            "licitacion_no_encontrada");
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Post_ConFechaVencida_DevuelveUnprocessableSinGuardar()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(-1));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsync(
            $"/api/v1/licitaciones/{licitacion.Id}/publicar",
            content: null);

        await VerificarProblemaAsync(
            respuesta,
            HttpStatusCode.UnprocessableEntity,
            "licitacion_datos_no_publicables");
        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Post_ConLicitacionPublicada_DevuelveConflictSinGuardar()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(5));
        licitacion.Publicar(DateTimeOffset.UtcNow);
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsync(
            $"/api/v1/licitaciones/{licitacion.Id}/publicar",
            content: null);

        await VerificarProblemaAsync(
            respuesta,
            HttpStatusCode.Conflict,
            "licitacion_estado_no_publicable");
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Post_ConConflictoDeConcurrencia_DevuelveConflictSeguro()
    {
        var licitacion = CrearLicitacion(DateTimeOffset.UtcNow.AddDays(5));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion)
        {
            SimularConflicto = true
        };

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsync(
            $"/api/v1/licitaciones/{licitacion.Id}/publicar",
            content: null);

        var problema = await VerificarProblemaAsync(
            respuesta,
            HttpStatusCode.Conflict,
            "licitacion_conflicto_concurrencia");
        Assert.DoesNotContain("DbUpdate", problema.Detail);
        Assert.DoesNotContain("xmin", problema.Detail);
    }

    [Fact]
    public async Task OpenApi_DocumentaPublicacionSinCuerpoYRespuestas()
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
            .GetProperty("/api/v1/licitaciones/{id}/publicar")
            .GetProperty("post");
        var respuestas = operacion.GetProperty("responses");

        Assert.False(operacion.TryGetProperty("requestBody", out _));
        Assert.True(respuestas.TryGetProperty("200", out _));
        Assert.True(respuestas.TryGetProperty("400", out _));
        Assert.True(respuestas.TryGetProperty("404", out _));
        Assert.True(respuestas.TryGetProperty("409", out _));
        Assert.True(respuestas.TryGetProperty("422", out _));
        Assert.True(respuestas.TryGetProperty("500", out _));
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

    private static async Task<ProblemaRespuesta> VerificarProblemaAsync(
        HttpResponseMessage respuesta,
        HttpStatusCode estado,
        string codigoError)
    {
        Assert.Equal(estado, respuesta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal((int)estado, problema.Status);
        Assert.Equal(codigoError, problema.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(problema.Detail));
        Assert.False(string.IsNullOrWhiteSpace(problema.CorrelationId));
        return problema;
    }

    private sealed record PublicarLicitacionRespuesta(
        Guid Id,
        string Codigo,
        string Titulo,
        decimal PresupuestoEstimadoCrc,
        DateTimeOffset FechaCierre,
        string Estado,
        DateTimeOffset UpdatedAt,
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
            Guid licitacionId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                licitacion?.Id == licitacionId ? licitacion : null);
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
