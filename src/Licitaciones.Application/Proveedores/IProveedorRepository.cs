using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public interface IProveedorRepository
{
    Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Proveedor?>(null);
    Task<bool> ExisteConNombreNormalizadoAsync(
        string nombreNormalizado,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default);
}
