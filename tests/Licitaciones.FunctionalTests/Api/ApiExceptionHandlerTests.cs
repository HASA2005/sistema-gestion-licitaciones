using System.Text.Json;
using Licitaciones.Api.Errors;
using Licitaciones.Application.Aprobaciones;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Licitaciones;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Licitaciones.FunctionalTests.Api;

public sealed class ApiExceptionHandlerTests
{
    [Fact]
    public async Task Api_RecursoInexistente_DevuelveNotFoundProblemDetails()
    {
        await using var aplicacion = new ApiFactory();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.GetAsync($"/api/v1/licitaciones/{Guid.NewGuid()}");

        Assert.Equal(StatusCodes.Status404NotFound, (int)respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        Assert.Equal("recurso_no_encontrado", documento.RootElement.GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData(typeof(LicitacionNoEncontradaException))]
    [InlineData(typeof(OfertaNoEncontradaException))]
    [InlineData(typeof(KeyNotFoundException))]
    public async Task RecursoInexistente_DevuelveNotFoundProblemDetails(
        Type tipoExcepcion)
    {
        var resultado = await EjecutarAsync((Exception)Activator.CreateInstance(tipoExcepcion)!);

        Assert.Equal(StatusCodes.Status404NotFound, resultado.Status);
        Assert.Equal("recurso_no_encontrado", resultado.ErrorCode);
        Assert.Equal("application/problem+json", resultado.ContentType);
    }

    [Theory]
    [InlineData(typeof(ArgumentException))]
    [InlineData(typeof(OfertaReglaException))]
    [InlineData(typeof(RangoAprobacionTraslapadoException))]
    [InlineData(typeof(LicitacionConOfertasException))]
    [InlineData(typeof(ProveedorConOfertasException))]
    public async Task ReglaDeNegocio_DevuelveBadRequestProblemDetails(
        Type tipoExcepcion)
    {
        var excepcion = tipoExcepcion == typeof(OfertaReglaException)
            ? new OfertaReglaException("Regla inválida.")
            : (Exception)Activator.CreateInstance(tipoExcepcion)!;
        var resultado = await EjecutarAsync(excepcion);

        Assert.Equal(StatusCodes.Status400BadRequest, resultado.Status);
        Assert.Equal("regla_negocio_invalida", resultado.ErrorCode);
        Assert.Equal("application/problem+json", resultado.ContentType);
    }

    [Theory]
    [InlineData(typeof(OfertaDuplicadaException))]
    [InlineData(typeof(ProveedorDuplicadoException))]
    [InlineData(typeof(LicitacionDuplicadaException))]
    [InlineData(typeof(LicitacionConcurrenciaException))]
    [InlineData(typeof(TipoCambioActivoException))]
    public async Task ConflictoDeNegocio_DevuelveConflictProblemDetails(
        Type tipoExcepcion)
    {
        var resultado = await EjecutarAsync((Exception)Activator.CreateInstance(tipoExcepcion)!);

        Assert.Equal(StatusCodes.Status409Conflict, resultado.Status);
        Assert.Equal("conflicto_negocio", resultado.ErrorCode);
        Assert.Equal("application/problem+json", resultado.ContentType);
    }

    [Fact]
    public async Task ErrorInesperado_ConservaInternalServerError()
    {
        var resultado = await EjecutarAsync(
            new InvalidOperationException("error técnico inesperado"));

        Assert.Equal(StatusCodes.Status500InternalServerError, resultado.Status);
        Assert.Equal("error_interno", resultado.ErrorCode);
        Assert.Equal("application/problem+json", resultado.ContentType);
    }

    private static async Task<Problema> EjecutarAsync(Exception excepcion)
    {
        var respuesta = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() }
        };
        var manejador = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);

        var manejado = await manejador.TryHandleAsync(
            respuesta,
            excepcion,
            CancellationToken.None);

        Assert.True(manejado);
        respuesta.Response.Body.Position = 0;
        using var documento = await JsonDocument.ParseAsync(respuesta.Response.Body);
        var raiz = documento.RootElement;
        return new Problema(
            respuesta.Response.StatusCode,
            raiz.GetProperty("errorCode").GetString()!,
            respuesta.Response.ContentType!);
    }

    private sealed record Problema(int Status, string ErrorCode, string ContentType);

    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:Licitaciones",
                "Host=localhost;Database=licitaciones_tests;Username=test;Password=test");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ILicitacionRepository>();
                services.AddSingleton<ILicitacionRepository>(new RepositorioLicitacionesVacio());
            });
        }
    }

    private sealed class RepositorioLicitacionesVacio : ILicitacionRepository
    {
        public Task<bool> ExisteConCodigoNormalizadoAsync(
            string codigoNormalizado,
            CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task AgregarAsync(
            Licitacion licitacion,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<Licitacion?> ObtenerPorIdAsync(
            Guid licitacionId,
            CancellationToken cancellationToken = default) => Task.FromResult<Licitacion?>(null);

        public Task GuardarCambiosAsync(
            Licitacion licitacion,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
