using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Proveedores;

namespace Licitaciones.UnitTests.Application.Proveedores;

public sealed class RegistrarProveedorServiceTests
{
    [Fact]
    public async Task Registrar_ConNombreValido_GuardaProveedorYDevuelveConfirmacion()
    {
        var repositorio = new RepositorioProveedoresEnMemoria();
        var servicio = new RegistrarProveedorService(repositorio);

        var resultado = await servicio.EjecutarAsync("  Empresa   Central  ");

        Assert.Equal("Proveedor registrado correctamente.", resultado.Mensaje);

        var proveedorGuardado = Assert.Single(repositorio.Proveedores);
        Assert.Equal("Empresa Central", proveedorGuardado.Nombre);
        Assert.Equal("EMPRESA CENTRAL", proveedorGuardado.NombreNormalizado);
    }

    [Fact]
    public async Task Registrar_ConNombreValido_GeneraIdentificadorYAuditoriaUtc()
    {
        var fechaLocal = new DateTimeOffset(
            2026,
            8,
            23,
            10,
            30,
            0,
            TimeSpan.FromHours(-6));
        var repositorio = new RepositorioProveedoresEnMemoria();
        var servicio = new RegistrarProveedorService(
            repositorio,
            new RelojFijo(fechaLocal));

        await servicio.EjecutarAsync("Empresa Central");

        var proveedorGuardado = Assert.Single(repositorio.Proveedores);
        Assert.NotEqual(Guid.Empty, proveedorGuardado.Id);
        Assert.Equal(fechaLocal.ToUniversalTime(), proveedorGuardado.CreatedAt);
        Assert.Equal(proveedorGuardado.CreatedAt, proveedorGuardado.UpdatedAt);
        Assert.Equal(TimeSpan.Zero, proveedorGuardado.CreatedAt.Offset);
    }

    [Theory]
    [InlineData("Empresa Central", "Empresa Central")]
    [InlineData("Empresa Central", " empresa   central ")]
    [InlineData("Café Central", "Cafe\u0301 Central")]
    public async Task Registrar_ConNombreDuplicado_LanzaErrorControladoYNoGuarda(
        string nombreRegistrado,
        string nombreDuplicado)
    {
        var repositorio = new RepositorioProveedoresEnMemoria();
        await repositorio.AgregarAsync(new Proveedor(nombreRegistrado));
        var servicio = new RegistrarProveedorService(repositorio);

        var excepcion = await Assert.ThrowsAsync<ProveedorDuplicadoException>(
            () => servicio.EjecutarAsync(nombreDuplicado));

        Assert.Equal(
            "Ya existe un proveedor con el mismo nombre.",
            excepcion.Message);
        Assert.Single(repositorio.Proveedores);
    }

    private sealed class RepositorioProveedoresEnMemoria : IProveedorRepository
    {
        public List<Proveedor> Proveedores { get; } = [];

        public Task<bool> ExisteConNombreNormalizadoAsync(
            string nombreNormalizado,
            CancellationToken cancellationToken = default)
        {
            var existe = Proveedores.Any(
                proveedor => proveedor.NombreNormalizado.Equals(
                    nombreNormalizado,
                    StringComparison.Ordinal));

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

    private sealed class RelojFijo(DateTimeOffset fechaActual) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return fechaActual;
        }
    }
}
