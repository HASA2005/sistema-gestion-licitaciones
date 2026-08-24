using System.Text;
using System.Text.RegularExpressions;

namespace Licitaciones.Domain.Proveedores;

public sealed class Proveedor
{
    private const string PatronNombrePermitido = @"^[\p{L}\p{N} .,\(\)]+$";

    private Proveedor()
    {
        Nombre = string.Empty;
        NombreNormalizado = string.Empty;
    }

    public Proveedor(string nombre)
        : this(nombre, TimeProvider.System.GetUtcNow())
    {
    }

    public Proveedor(string nombre, DateTimeOffset fechaCreacion)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException(
                "El nombre del proveedor es obligatorio.",
                nameof(nombre));
        }

        var nombreNormalizadoUnicode = nombre.Normalize(NormalizationForm.FormC);

        if (!Regex.IsMatch(nombreNormalizadoUnicode, PatronNombrePermitido))
        {
            throw new ArgumentException(
                "El nombre del proveedor contiene caracteres no permitidos.",
                nameof(nombre));
        }

        Nombre = string.Join(
            ' ',
            nombreNormalizadoUnicode
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries));
        NombreNormalizado = Nombre.ToUpperInvariant();
        Id = Guid.NewGuid();
        CreatedAt = fechaCreacion.ToUniversalTime();
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string Nombre { get; private set; }

    public string NombreNormalizado { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public uint Version { get; private set; }
}
