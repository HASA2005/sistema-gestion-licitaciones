using Licitaciones.Application.Licitaciones;
using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class PublicarLicitacionServiceTests
{
    private static readonly DateTimeOffset FechaActual =
        new(2026, 8, 25, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Publicar_ConBorradorValido_GuardaYDevuelveConfirmacion()
    {
        var licitacion = CrearLicitacion(FechaActual.AddDays(1));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);
        var servicio = new PublicarLicitacionService(
            repositorio,
            new RelojFijo(FechaActual));

        var resultado = await servicio.EjecutarAsync(licitacion.Id);

        Assert.Equal(licitacion.Id, resultado.Id);
        Assert.Equal("LIC-001", resultado.Codigo);
        Assert.Equal("Compra de equipo", resultado.Titulo);
        Assert.Equal(1_000m, resultado.PresupuestoEstimadoCrc);
        Assert.Equal(FechaActual.AddDays(1), resultado.FechaCierre);
        Assert.Equal(EstadoLicitacion.Publicada, resultado.Estado);
        Assert.Equal(FechaActual, resultado.UpdatedAt);
        Assert.Equal("Licitación publicada correctamente.", resultado.Mensaje);
        Assert.Equal(licitacion.Id, repositorio.UltimoIdConsultado);
        Assert.Same(licitacion, repositorio.LicitacionGuardada);
        Assert.Equal(1, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Consultar_ConLicitacionExistente_DevuelveDatosSinGuardar()
    {
        var licitacion = CrearLicitacion(FechaActual.AddDays(1));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);
        var servicio = new PublicarLicitacionService(
            repositorio,
            new RelojFijo(FechaActual));

        var resultado = await servicio.ConsultarAsync(licitacion.Id);

        Assert.Equal(licitacion.Id, resultado.Id);
        Assert.Equal("LIC-001", resultado.Codigo);
        Assert.Equal("Compra de equipo", resultado.Titulo);
        Assert.Equal(1_000m, resultado.PresupuestoEstimadoCrc);
        Assert.Equal(FechaActual.AddDays(1), resultado.FechaCierre);
        Assert.Equal(EstadoLicitacion.Borrador, resultado.Estado);
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Publicar_ConIdInexistente_LanzaErrorYNoGuarda()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();
        var servicio = new PublicarLicitacionService(
            repositorio,
            new RelojFijo(FechaActual));

        var excepcion = await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => servicio.EjecutarAsync(Guid.NewGuid()));

        Assert.Equal(
            "No se encontró la licitación solicitada.",
            excepcion.Message);
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Publicar_ConCierreVencido_LanzaErrorYNoGuarda()
    {
        var licitacion = CrearLicitacion(FechaActual);
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);
        var servicio = new PublicarLicitacionService(
            repositorio,
            new RelojFijo(FechaActual));

        await Assert.ThrowsAsync<PublicacionLicitacionInvalidaException>(
            () => servicio.EjecutarAsync(licitacion.Id));

        Assert.Equal(EstadoLicitacion.Borrador, licitacion.Estado);
        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Consultar_ConIdInexistente_LanzaErrorYNoGuarda()
    {
        var repositorio = new RepositorioLicitacionesEnMemoria();
        var servicio = new PublicarLicitacionService(
            repositorio,
            new RelojFijo(FechaActual));

        await Assert.ThrowsAsync<LicitacionNoEncontradaException>(
            () => servicio.ConsultarAsync(Guid.NewGuid()));

        Assert.Equal(0, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Publicar_ConConflictoDeConcurrencia_PropagaErrorSeguro()
    {
        var licitacion = CrearLicitacion(FechaActual.AddDays(1));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion)
        {
            SimularConflicto = true
        };
        var servicio = new PublicarLicitacionService(
            repositorio,
            new RelojFijo(FechaActual));

        var excepcion = await Assert.ThrowsAsync<LicitacionConcurrenciaException>(
            () => servicio.EjecutarAsync(licitacion.Id));

        Assert.Equal(
            "La licitación fue modificada por otra operación. Actualice los datos e intente nuevamente.",
            excepcion.Message);
        Assert.DoesNotContain("DbUpdate", excepcion.Message);
        Assert.DoesNotContain("xmin", excepcion.Message);
        Assert.Equal(1, repositorio.CantidadGuardados);
    }

    [Fact]
    public async Task Publicar_PropagaCancellationTokenAConsultaYGuardado()
    {
        var licitacion = CrearLicitacion(FechaActual.AddDays(1));
        var repositorio = new RepositorioLicitacionesEnMemoria(licitacion);
        var servicio = new PublicarLicitacionService(
            repositorio,
            new RelojFijo(FechaActual));
        using var origenCancelacion = new CancellationTokenSource();

        await servicio.EjecutarAsync(
            licitacion.Id,
            origenCancelacion.Token);

        Assert.Equal(origenCancelacion.Token, repositorio.TokenConsulta);
        Assert.Equal(origenCancelacion.Token, repositorio.TokenGuardado);
    }

    private static Licitacion CrearLicitacion(DateTimeOffset fechaCierre)
    {
        return new Licitacion(
            "LIC-001",
            "Compra de equipo",
            1_000m,
            fechaCierre,
            FechaActual.AddDays(-1));
    }

    private sealed class RepositorioLicitacionesEnMemoria(
        Licitacion? licitacion = null) : ILicitacionRepository
    {
        public bool SimularConflicto { get; init; }

        public Guid? UltimoIdConsultado { get; private set; }

        public Licitacion? LicitacionGuardada { get; private set; }

        public int CantidadGuardados { get; private set; }

        public CancellationToken TokenConsulta { get; private set; }

        public CancellationToken TokenGuardado { get; private set; }

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
            UltimoIdConsultado = licitacionId;
            TokenConsulta = cancellationToken;
            return Task.FromResult(
                licitacion?.Id == licitacionId ? licitacion : null);
        }

        public Task GuardarCambiosAsync(
            Licitacion licitacionModificada,
            CancellationToken cancellationToken = default)
        {
            LicitacionGuardada = licitacionModificada;
            TokenGuardado = cancellationToken;
            CantidadGuardados++;
            if (SimularConflicto)
            {
                throw new LicitacionConcurrenciaException();
            }

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
