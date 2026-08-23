using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public interface IProveedorRepository
{
    Task AgregarAsync(
        Proveedor proveedor,
        CancellationToken cancellationToken = default);
}
