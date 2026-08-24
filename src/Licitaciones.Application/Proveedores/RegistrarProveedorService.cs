using Licitaciones.Domain.Proveedores;

namespace Licitaciones.Application.Proveedores;

public sealed class RegistrarProveedorService
{
    private readonly IProveedorRepository _repositorio;
    private readonly TimeProvider _reloj;

    public RegistrarProveedorService(IProveedorRepository repositorio)
        : this(repositorio, TimeProvider.System)
    {
    }

    public RegistrarProveedorService(
        IProveedorRepository repositorio,
        TimeProvider reloj)
    {
        _repositorio = repositorio;
        _reloj = reloj;
    }

    public async Task<RegistrarProveedorResultado> EjecutarAsync(
        string nombre,
        CancellationToken cancellationToken = default)
    {
        var proveedor = new Proveedor(nombre, _reloj.GetUtcNow());

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
