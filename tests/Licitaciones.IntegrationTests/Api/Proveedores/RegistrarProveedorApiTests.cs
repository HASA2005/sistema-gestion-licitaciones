using System.Net;
using System.Net.Http.Json;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Api.Proveedores;

public sealed class RegistrarProveedorApiTests
{
    [Fact]
    public async Task Post_ConInfraestructuraReal_PersisteProveedor()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("licitaciones")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await postgres.StartAsync();

        var opciones = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using (var contexto = new LicitacionesDbContext(opciones))
        {
            await contexto.Database.MigrateAsync();
        }

        await using var aplicacion = new ApiFactory(
            postgres.GetConnectionString());
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/proveedores",
            new { nombre = "Empresa Central" });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var proveedorGuardado = await contextoVerificacion.Proveedores
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal("Empresa Central", proveedorGuardado.Nombre);
        Assert.Equal("EMPRESA CENTRAL", proveedorGuardado.NombreNormalizado);
    }

    private sealed class ApiFactory(
        string cadenaConexion) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting(
                "ConnectionStrings:Licitaciones",
                cadenaConexion);
            builder.ConfigureLogging(logging => logging.ClearProviders());
        }
    }
}
