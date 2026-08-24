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
    public async Task GuardarCambiosAsync_AlPublicar_PersisteEstadoAuditoriaYCambiaXmin()
    {
        await using var postgres = CrearPostgres();
        await postgres.StartAsync();

        var opciones = CrearOpciones(postgres.GetConnectionString());
        var fechaPublicacion = new DateTimeOffset(
            2026,
            8,
            25,
            9,
            30,
            0,
            TimeSpan.FromHours(-6));
        var licitacion = CrearLicitacion("LIC-2026-002");
        var licitacionId = licitacion.Id;
        uint versionBorrador;

        await using (var contextoInicial = new LicitacionesDbContext(opciones))
        {
            await contextoInicial.Database.EnsureCreatedAsync();
            var repositorio = new LicitacionRepository(contextoInicial);
            await repositorio.AgregarAsync(licitacion);
            versionBorrador = licitacion.Version;
        }

        await using (var contextoPublicacion =
            new LicitacionesDbContext(opciones))
        {
            var repositorio = new LicitacionRepository(contextoPublicacion);
            var borrador = Assert.IsType<Licitacion>(
                await repositorio.ObtenerPorIdAsync(licitacionId));

            borrador.Publicar(fechaPublicacion);
            await repositorio.GuardarCambiosAsync(borrador);
        }

        await using var contextoVerificacion =
            new LicitacionesDbContext(opciones);
        var guardada = await contextoVerificacion.Licitaciones
            .AsNoTracking()
            .SingleAsync(actual => actual.Id == licitacionId);

        Assert.Equal(EstadoLicitacion.Publicada, guardada.Estado);
        Assert.Equal(fechaPublicacion.ToUniversalTime(), guardada.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, guardada.UpdatedAt.Offset);
        Assert.Equal(FechaCreacion, guardada.CreatedAt);
        Assert.NotEqual(versionBorrador, guardada.Version);
    }

    [Fact]
    public async Task GuardarCambiosAsync_ConXminObsoleto_TraduceConflictoYConservaPrimerCambio()
    {
        await using var postgres = CrearPostgres();
        await postgres.StartAsync();

        var opciones = CrearOpciones(postgres.GetConnectionString());
        var fechaPrimeraPublicacion = new DateTimeOffset(
            2026,
            8,
            25,
            15,
            0,
            0,
            TimeSpan.Zero);
        var fechaSegundaPublicacion = fechaPrimeraPublicacion.AddMinutes(5);
        var licitacion = CrearLicitacion("LIC-2026-003");
        var licitacionId = licitacion.Id;

        await using (var contextoInicial = new LicitacionesDbContext(opciones))
        {
            await contextoInicial.Database.EnsureCreatedAsync();
            var repositorio = new LicitacionRepository(contextoInicial);
            await repositorio.AgregarAsync(licitacion);
        }

        await using var primerContexto = new LicitacionesDbContext(opciones);
        await using var segundoContexto = new LicitacionesDbContext(opciones);
        var primerRepositorio = new LicitacionRepository(primerContexto);
        var segundoRepositorio = new LicitacionRepository(segundoContexto);
        var primeraCopia = Assert.IsType<Licitacion>(
            await primerRepositorio.ObtenerPorIdAsync(licitacionId));
        var segundaCopia = Assert.IsType<Licitacion>(
            await segundoRepositorio.ObtenerPorIdAsync(licitacionId));

        primeraCopia.Publicar(fechaPrimeraPublicacion);
        segundaCopia.Publicar(fechaSegundaPublicacion);

        await primerRepositorio.GuardarCambiosAsync(primeraCopia);
        var excepcion = await Assert.ThrowsAsync<LicitacionConcurrenciaException>(
            () => segundoRepositorio.GuardarCambiosAsync(segundaCopia));

        Assert.Equal(
            "La licitación fue modificada por otra operación. " +
            "Actualice los datos e intente nuevamente.",
            excepcion.Message);

        await using var tercerContexto = new LicitacionesDbContext(opciones);
        var guardada = await tercerContexto.Licitaciones
            .AsNoTracking()
            .SingleAsync(actual => actual.Id == licitacionId);

        Assert.Equal(EstadoLicitacion.Publicada, guardada.Estado);
        Assert.Equal(fechaPrimeraPublicacion, guardada.UpdatedAt);
        Assert.NotEqual(fechaSegundaPublicacion, guardada.UpdatedAt);
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
