namespace Licitaciones.Application.Proveedores;

public sealed class ProveedorDuplicadoException : Exception
{
    public ProveedorDuplicadoException()
        : base("Ya existe un proveedor con el mismo nombre.")
    {
    }
}
