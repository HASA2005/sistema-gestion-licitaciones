namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Indica que no existe la licitación solicitada.
/// </summary>
public sealed class LicitacionNoEncontradaException : Exception
{
    /// <summary>
    /// Inicializa la excepción con el mensaje controlado del caso de uso.
    /// </summary>
    public LicitacionNoEncontradaException()
        : base("No se encontró la licitación solicitada.")
    {
    }
}
