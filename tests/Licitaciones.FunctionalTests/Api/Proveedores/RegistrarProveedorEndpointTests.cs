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

    private sealed record RegistrarProveedorRespuesta(string Mensaje);

    private sealed class ApiFactory(
        IProveedorRepository repositorio) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
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
