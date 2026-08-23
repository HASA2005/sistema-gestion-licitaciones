using System.Text;
using System.Text.RegularExpressions;

namespace Licitaciones.Domain.Proveedores;

public sealed class Proveedor
{
    private const string PatronNombrePermitido = @"^[\p{L}\p{N} .,\(\)]+$";

    public Proveedor(string nombre)
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

        NombreNormalizado = string.Join(
            ' ',
            nombreNormalizadoUnicode
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
    }

    public string NombreNormalizado { get; }
}
