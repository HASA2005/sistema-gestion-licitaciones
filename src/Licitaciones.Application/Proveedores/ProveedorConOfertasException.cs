namespace Licitaciones.Application.Proveedores;

public sealed class ProveedorConOfertasException : InvalidOperationException
{
    public ProveedorConOfertasException()
        : base("No se puede eliminar un proveedor con ofertas.")
    {
    }
}
