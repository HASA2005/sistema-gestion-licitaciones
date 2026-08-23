namespace Licitaciones.Domain.Proveedores;

public sealed class Proveedor
{
    public Proveedor(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del proveedor es obligatorio.",
                nameof(nombre));
        }

        NombreNormalizado = string.Join(
            ' ',
            nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    public string NombreNormalizado { get; }
}
