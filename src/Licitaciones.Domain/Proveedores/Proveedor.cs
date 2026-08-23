namespace Licitaciones.Domain.Proveedores;

public sealed class Proveedor
{
    public Proveedor(string nombre)
    {
        NombreNormalizado = string.Join(
            ' ',
            nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    public string NombreNormalizado { get; }
}
