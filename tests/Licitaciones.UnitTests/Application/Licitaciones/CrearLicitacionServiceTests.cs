using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class CrearLicitacionServiceTests
{
    [Fact]
    public async Task Crear_ConDatosValidos_GuardaLicitacionYDevuelveResultadoCompleto()
    {
        var fechaCreacion = new DateTimeOffset(
            2026,
            8,
            24,
            9,
            15,
            0,
            TimeSpan.FromHours(-6));
        var fechaCierre = new DateTimeOffset(
            2026,
            10,
            1,
            16,
            30,
            0,
            TimeSpan.FromHours(-6));
        var repositorio = new RepositorioLicitacionesEnMemoria();
        var servicio = new CrearLicitacionService(
            repositorio,
            new RelojFijo(fechaCreacion));
        var comando = new CrearLicitacionComando(
            "  LiC  001  ",
            "  Compra de equipo  ",
            1_500_000.25m,
            fechaCierre);

        var resultado = await servicio.EjecutarAsync(comando);

        Assert.NotEqual(Guid.Empty, resultado.Id);
        Assert.Equal("LiC  001", resultado.Codigo);
        Assert.Equal("Compra de equipo", resultado.Titulo);
        Assert.Equal(1_500_000.25m, resultado.PresupuestoEstimadoCrc);
        Assert.Equal(fechaCierre.ToUniversalTime(), resultado.FechaCierre);
        Assert.Equal(EstadoLicitacion.Borrador, resultado.Estado);
        Assert.Equal("Licitación creada correctamente.", resultado.Mensaje);
        Assert.Equal("LIC  001", repositorio.UltimoCodigoConsultado);

        var licitacionGuardada = Assert.Single(repositorio.Licitaciones);
        Assert.Equal(resultado.Id, licitacionGuardada.Id);
        Assert.Equal(fechaCreacion.ToUniversalTime(), licitacionGuardada.CreatedAt);
        Assert.Equal(
            licitacionGuardada.CreatedAt,
            licitacionGuardada.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, licitacionGuardada.CreatedAt.Offset);
    }

    [Theory]
    [InlineData("LIC-001")]
    [InlineData(" lic-001 ")]
    [InlineData("LiC-001")]
    public async Task Crear_ConCodigoDuplicado_LanzaErrorControladoYNoGuarda(
        string codigoDuplicado)
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();
        await repositorio.AgregarAsync(new Licitacion(
            "LIC-001",
            "Compra existente",
            1_000m,
            FechaCierreValida()));
        var servicio = new CrearLicitacionService(repositorio);
        var comando = new CrearLicitacionComando(
            codigoDuplicado,
            "Nueva compra",
            2_000m,
            FechaCierreValida());

        var excepcion = await Assert.ThrowsAsync<LicitacionDuplicadaException>(
            () => servicio.EjecutarAsync(comando));

        Assert.Equal(
            "Ya existe una licitación con el mismo código.",
            excepcion.Message);
        Assert.Single(repositorio.Licitaciones);
    }

    private static DateTimeOffset FechaCierreValida()
    {
        return new DateTimeOffset(
            2026,
            10,
            1,
            18,
            0,
            0,
            TimeSpan.Zero);
    }

    private sealed class RepositorioLicitacionesEnMemoria : ILicitacionRepository
    {
        public List<Licitacion> Licitaciones { get; } = [];

        public string? UltimoCodigoConsultado { get; private set; }

        public Task<bool> ExisteConCodigoNormalizadoAsync(
            string codigoNormalizado,
            CancellationToken cancellationToken = default)
        {
            UltimoCodigoConsultado = codigoNormalizado;
            var existe = Licitaciones.Any(
                licitacion => licitacion.CodigoNormalizado.Equals(
                    codigoNormalizado,
                    StringComparison.Ordinal));

            return Task.FromResult(existe);
        }

        public Task AgregarAsync(
            Licitacion licitacion,
            CancellationToken cancellationToken = default)
        {
            Licitaciones.Add(licitacion);
            return Task.CompletedTask;
        }
    }

    private sealed class RelojFijo(DateTimeOffset fechaActual) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return fechaActual;
        }
    }
}
