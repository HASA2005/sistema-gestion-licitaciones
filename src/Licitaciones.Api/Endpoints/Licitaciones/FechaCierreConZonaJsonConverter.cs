using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Licitaciones.Api.Endpoints.Licitaciones;

internal sealed class FechaCierreConZonaJsonConverter
    : JsonConverter<DateTimeOffset?>
{
    /// <inheritdoc />
    public override DateTimeOffset? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException(
                "La fecha de cierre debe ser una cadena ISO 8601 con zona horaria.");
        }

        var texto = reader.GetString()?.Trim();
        if (string.IsNullOrEmpty(texto) || !TieneZonaExplicita(texto))
        {
            throw new JsonException(
                "La fecha de cierre debe incluir Z o un desplazamiento horario explícito.");
        }

        if (!DateTimeOffset.TryParse(
                texto,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
        {
            throw new JsonException(
                "La fecha de cierre no tiene un formato ISO 8601 válido.");
        }

        return fecha;
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        DateTimeOffset? value,
        JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
            return;
        }

        writer.WriteNullValue();
    }

    private static bool TieneZonaExplicita(string texto)
    {
        if (texto.EndsWith('Z') || texto.EndsWith('z'))
        {
            return true;
        }

        if (texto.Length < 6)
        {
            return false;
        }

        var inicio = texto.Length - 6;
        return (texto[inicio] == '+' || texto[inicio] == '-')
            && char.IsDigit(texto[inicio + 1])
            && char.IsDigit(texto[inicio + 2])
            && texto[inicio + 3] == ':'
            && char.IsDigit(texto[inicio + 4])
            && char.IsDigit(texto[inicio + 5]);
    }
}
