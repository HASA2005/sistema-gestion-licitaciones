using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Persistence;

public sealed class LicitacionRepositoryTests
{
    private static readonly DateTimeOffset FechaCreacion =
        new(2026, 8, 24, 15, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset FechaCierre =
        new(2026, 9, 30, 22, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AgregarYConsultarAsync_PersisteLicitacionEnPostgreSql()
    {
        await using var postgres = CrearPostgres();
        await postgres.StartAsync();

        var opciones = CrearOpciones(postgres.GetConnectionString());

        await using (var contexto = new LicitacionesDbContext(opciones))
        {
            await contexto.Database.EnsureCreatedAsync();

            var repositorio = new LicitacionRepository(contexto);
            await repositorio.AgregarAsync(new Licitacion(
                "  Lic-2026-001  ",
                "  Compra de equipo  ",
                1_250_000.50m,
                FechaCierre,
                FechaCreacion));
        }

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var repositorioVerificacion =
            new LicitacionRepository(contextoVerificacion);

        var existe = await repositorioVerificacion
            .ExisteConCodigoNormalizadoAsync("LIC-2026-001");
        var guardada = await contextoVerificacion.Licitaciones
            .AsNoTracking()
            .SingleAsync();

        Assert.True(existe);
        Assert.Equal("Lic-2026-001", guardada.Codigo);
        Assert.Equal("LIC-2026-001", guardada.CodigoNormalizado);
        Assert.Equal("Compra de equipo", guardada.Titulo);
        Assert.Equal(1_250_000.50m, guardada.PresupuestoEstimadoCrc);
        Assert.Equal(FechaCierre, guardada.FechaCierre);
        Assert.Equal(EstadoLicitacion.Borrador, guardada.Estado);
        Assert.Equal(FechaCreacion, guardada.CreatedAt);
        Assert.NotEqual(0u, guardada.Version);
    }

    [Fact]
    public async Task AgregarAsync_EnCarreraDeCodigoDuplicado_LanzaErrorControlado()
    {
        await using var postgres = CrearPostgres();
        await postgres.StartAsync();

        var opciones = CrearOpciones(postgres.GetConnectionString());

        await using (var contextoInicial = new LicitacionesDbContext(opciones))
        {
            await contextoInicial.Database.EnsureCreatedAsync();
        }

        await using var primerContexto = new LicitacionesDbContext(opciones);
        await using var segundoContexto = new LicitacionesDbContext(opciones);
        var primerRepositorio = new LicitacionRepository(primerContexto);
        var segundoRepositorio = new LicitacionRepository(segundoContexto);

        Assert.False(await primerRepositorio
            .ExisteConCodigoNormalizadoAsync("LIC-2026-001"));
        Assert.False(await segundoRepositorio
            .ExisteConCodigoNormalizadoAsync("LIC-2026-001"));

        await primerRepositorio.AgregarAsync(CrearLicitacion("LIC-2026-001"));

        var excepcion = await Assert.ThrowsAsync<LicitacionDuplicadaException>(
            () => segundoRepositorio.AgregarAsync(
                CrearLicitacion("  lic-2026-001  ")));

        Assert.Equal(
            "Ya existe una licitación con el mismo código.",
            excepcion.Message);

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        Assert.Equal(1, await contextoVerificacion.Licitaciones.CountAsync());
    }

    [Fact]
    public async Task Guardar_ConVersionXminObsoleta_LanzaConflictoDeConcurrencia()
    {
        await using var postgres = CrearPostgres();
        await postgres.StartAsync();

        var opciones = CrearOpciones(postgres.GetConnectionString());
        await using (var contextoInicial = new LicitacionesDbContext(opciones))
        {
            await contextoInicial.Database.EnsureCreatedAsync();
            var repositorio = new LicitacionRepository(contextoInicial);
            await repositorio.AgregarAsync(CrearLicitacion("LIC-2026-002"));
        }

        await using var primerContexto = new LicitacionesDbContext(opciones);
        await using var segundoContexto = new LicitacionesDbContext(opciones);
        var primeraCopia = await primerContexto.Licitaciones.SingleAsync();
        var segundaCopia = await segundoContexto.Licitaciones.SingleAsync();

        primerContexto.Entry(primeraCopia)
            .Property(licitacion => licitacion.Titulo)
            .CurrentValue = "Compra actualizada primero";
        await primerContexto.SaveChangesAsync();

        segundoContexto.Entry(segundaCopia)
            .Property(licitacion => licitacion.Titulo)
            .CurrentValue = "Compra actualizada después";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => segundoContexto.SaveChangesAsync());
    }

    private static Licitacion CrearLicitacion(string codigo)
    {
        return new Licitacion(
            codigo,
            "Compra de equipo",
            1_250_000m,
            FechaCierre,
            FechaCreacion);
    }

    private static PostgreSqlContainer CrearPostgres()
    {
        return new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("licitaciones")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    private static DbContextOptions<LicitacionesDbContext> CrearOpciones(
        string cadenaConexion)
    {
        return new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(cadenaConexion)
            .Options;
    }
}
