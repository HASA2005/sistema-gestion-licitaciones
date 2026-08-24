using System.Text;
using System.Text.RegularExpressions;

namespace Licitaciones.Domain.Proveedores;

public sealed class Proveedor
{
    private const string PatronNombrePermitido = @"^[\p{L}\p{N} .,\(\)]+$";

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

    public Guid Id { get; }

    public string Nombre { get; }

    public string NombreNormalizado { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; }
}
