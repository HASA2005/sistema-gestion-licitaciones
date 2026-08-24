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

public sealed class PublicarLicitacionApiTests
{
    [Fact]
    public async Task Post_ConInfraestructuraReal_PublicaBorradorYActualizaAuditoriaUtc()
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
        var fechaCreacion = DateTimeOffset.UtcNow.AddDays(-1);
        var fechaCierre = DateTimeOffset.FromUnixTimeSeconds(
            DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds());
        var licitacion = new Licitacion(
            "LIC-2030-INT-001",
            "Compra integrada de equipo",
            2_750_000.50m,
            fechaCierre,
            fechaCreacion);

        await using (var contexto = new LicitacionesDbContext(opciones))
        {
            await contexto.Database.MigrateAsync();
            await contexto.Licitaciones.AddAsync(licitacion);
            await contexto.SaveChangesAsync();
        }

        var updatedAtInicial = licitacion.UpdatedAt;
        var versionInicial = licitacion.Version;

        await using var aplicacion = new ApiFactory(
            postgres.GetConnectionString());
        using var cliente = aplicacion.CreateClient();
        var instanteAnterior = DateTimeOffset.UtcNow;

        var respuesta = await cliente.PostAsync(
            $"/api/v1/licitaciones/{licitacion.Id}/publicar",
            content: null);

        var instantePosterior = DateTimeOffset.UtcNow;
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        var contenido = await respuesta.Content
            .ReadFromJsonAsync<PublicarLicitacionRespuesta>();
        Assert.NotNull(contenido);
        Assert.Equal(licitacion.Id, contenido.Id);
        Assert.Equal("LIC-2030-INT-001", contenido.Codigo);
        Assert.Equal("Compra integrada de equipo", contenido.Titulo);
        Assert.Equal(2_750_000.50m, contenido.PresupuestoEstimadoCrc);
        Assert.Equal(fechaCierre, contenido.FechaCierre);
        Assert.Equal("Publicada", contenido.Estado);
        Assert.Equal(TimeSpan.Zero, contenido.UpdatedAt.Offset);
        Assert.InRange(
            contenido.UpdatedAt,
            instanteAnterior,
            instantePosterior);
        Assert.Equal(
            "Licitación publicada correctamente.",
            contenido.Mensaje);

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var guardada = await contextoVerificacion.Licitaciones
            .AsNoTracking()
            .SingleAsync(actual => actual.Id == licitacion.Id);

        Assert.Equal(EstadoLicitacion.Publicada, guardada.Estado);
        Assert.NotEqual(updatedAtInicial, guardada.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, guardada.UpdatedAt.Offset);
        Assert.InRange(
            guardada.UpdatedAt,
            instanteAnterior.AddMilliseconds(-1),
            instantePosterior);
        Assert.True(
            (guardada.UpdatedAt - contenido.UpdatedAt).Duration()
                < TimeSpan.FromMilliseconds(1));
        Assert.NotEqual(versionInicial, guardada.Version);
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
