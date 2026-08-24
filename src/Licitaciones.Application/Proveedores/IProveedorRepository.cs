using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public interface IProveedorRepository
{
    Task<bool> ExisteConNombreNormalizadoAsync(
        string nombreNormalizado,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default);
}
