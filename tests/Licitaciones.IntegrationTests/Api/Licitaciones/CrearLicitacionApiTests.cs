using System.Net;
using System.Net.Http.Json;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Api.Licitaciones;

public sealed class CrearLicitacionApiTests
{
    [Fact]
    public async Task Post_ConInfraestructuraReal_PersisteBorrador()
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
        var fechaCierre = new DateTimeOffset(
            2030,
            10,
            15,
            18,
            30,
            0,
            TimeSpan.FromHours(-6));

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/licitaciones",
            new
            {
                codigo = "  lic-2030-001  ",
                titulo = "  Compra de equipo informático  ",
                presupuestoEstimadoCrc = 1_250_000.50m,
                fechaCierre
            });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var guardada = await contextoVerificacion.Licitaciones
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal("lic-2030-001", guardada.Codigo);
        Assert.Equal("LIC-2030-001", guardada.CodigoNormalizado);
        Assert.Equal("Compra de equipo informático", guardada.Titulo);
        Assert.Equal(1_250_000.50m, guardada.PresupuestoEstimadoCrc);
        Assert.Equal(fechaCierre.ToUniversalTime(), guardada.FechaCierre);
        Assert.Equal(EstadoLicitacion.Borrador, guardada.Estado);
        Assert.NotEqual(0u, guardada.Version);
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
