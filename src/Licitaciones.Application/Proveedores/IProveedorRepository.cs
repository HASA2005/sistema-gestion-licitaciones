using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public interface IProveedorRepository
{
    Task<IReadOnlyList<Proveedor>> ListarAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Proveedor>>(Array.Empty<Proveedor>());
    Task EliminarAsync(Proveedor proveedor, CancellationToken cancellationToken = default) => Task.CompletedTask;
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Proveedor?>(null);
    Task<bool> ExisteConNombreNormalizadoAsync(
        string nombreNormalizado,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default);

    Task GuardarCambiosAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
