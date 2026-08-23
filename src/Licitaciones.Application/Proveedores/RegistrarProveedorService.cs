using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public sealed class RegistrarProveedorService
{
    private readonly IProveedorRepository _repositorio;

    public RegistrarProveedorService(IProveedorRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<RegistrarProveedorResultado> EjecutarAsync(
        string nombre,
        CancellationToken cancellationToken = default)
    {
        var proveedor = new Proveedor(nombre);

        var existeProveedor = await _repositorio.ExisteConNombreNormalizadoAsync(
            proveedor.NombreNormalizado,
            cancellationToken);

        if (existeProveedor)
        {
            throw new ProveedorDuplicadoException();
        }

        await _repositorio.AgregarAsync(proveedor, cancellationToken);

        return new RegistrarProveedorResultado(
            "Proveedor registrado correctamente.");
    }
}
