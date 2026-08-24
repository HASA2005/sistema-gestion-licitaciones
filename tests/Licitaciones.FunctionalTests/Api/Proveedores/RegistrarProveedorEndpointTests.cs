using System.Net;
using System.Net.Http.Json;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Api.Proveedores;

public sealed class RegistrarProveedorEndpointTests
{
    [Fact]
    public async Task Post_ConNombreValido_DevuelveCreatedYConfirmacion()
    {
        var repositorio = new RepositorioProveedoresEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/proveedores",
            new { nombre = "  Empresa   Central  " });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        var contenido = await respuesta.Content
            .ReadFromJsonAsync<RegistrarProveedorRespuesta>();
        Assert.NotNull(contenido);
        Assert.Equal("Proveedor registrado correctamente.", contenido.Mensaje);

        var proveedorGuardado = Assert.Single(repositorio.Proveedores);
        Assert.Equal("Empresa Central", proveedorGuardado.Nombre);
        Assert.Equal("EMPRESA CENTRAL", proveedorGuardado.NombreNormalizado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Empresa @ Central")]
    public async Task Post_ConNombreInvalido_DevuelveUnprocessableProblemDetails(
        string? nombre)
    {
        var repositorio = new RepositorioProveedoresEnMemoria();

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/proveedores",
            new { nombre });

        Assert.Equal(
            HttpStatusCode.UnprocessableEntity,
            respuesta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal("Datos del proveedor inválidos.", problema.Title);
        Assert.Equal(422, problema.Status);
        Assert.False(string.IsNullOrWhiteSpace(problema.Detail));
        Assert.Equal("proveedor_nombre_invalido", problema.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(problema.CorrelationId));
        Assert.Empty(repositorio.Proveedores);
    }

    [Fact]
    public async Task Post_ConNombreDuplicado_DevuelveConflictProblemDetails()
    {
        var repositorio = new RepositorioProveedoresEnMemoria
        {
            LanzarDuplicadoAlAgregar = true
        };

        await using var aplicacion = new ApiFactory(repositorio);
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/proveedores",
            new { nombre = "Empresa Central" });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            respuesta.Content.Headers.ContentType?.MediaType);

        var problema = await respuesta.Content
            .ReadFromJsonAsync<ProblemaRespuesta>();
        Assert.NotNull(problema);
        Assert.Equal("Proveedor duplicado.", problema.Title);
        Assert.Equal(409, problema.Status);
        Assert.Equal(
            "Ya existe un proveedor con el mismo nombre.",
            problema.Detail);
        Assert.Equal("proveedor_duplicado", problema.ErrorCode);
        Assert.False(string.IsNullOrWhiteSpace(problema.CorrelationId));
        Assert.Empty(repositorio.Proveedores);
    }

    private sealed record RegistrarProveedorRespuesta(string Mensaje);

    private sealed record ProblemaRespuesta(
        string Title,
        int Status,
        string Detail,
        string ErrorCode,
        string CorrelationId);

    private sealed class ApiFactory(
        IProveedorRepository repositorio) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:Licitaciones",
                "Host=localhost;Database=licitaciones_tests;Username=test;Password=test");
            builder.ConfigureLogging(logging => logging.ClearProviders());

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IProveedorRepository>();
                services.RemoveAll<RegistrarProveedorService>();
                services.AddSingleton(repositorio);
                services.AddScoped<RegistrarProveedorService>();
            });
        }
    }

    private sealed class RepositorioProveedoresEnMemoria
        : IProveedorRepository
    {
        public List<Proveedor> Proveedores { get; } = [];

        public bool LanzarDuplicadoAlAgregar { get; init; }

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
            if (LanzarDuplicadoAlAgregar)
            {
                throw new ProveedorDuplicadoException();
            }

            Proveedores.Add(proveedor);
            return Task.CompletedTask;
        }
    }
}
