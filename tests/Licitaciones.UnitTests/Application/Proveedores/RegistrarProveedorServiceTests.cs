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

    private sealed class RepositorioProveedoresEnMemoria : IProveedorRepository
    {
        public List<Proveedor> Proveedores { get; } = [];

        public Task AgregarAsync(
            Proveedor proveedor,
            CancellationToken cancellationToken = default)
        {
            Proveedores.Add(proveedor);
            return Task.CompletedTask;
        }
    }
}
