namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Excepción que se produce al intentar registrar un código de licitación existente.
/// </summary>
public sealed class LicitacionDuplicadaException : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="LicitacionDuplicadaException" />.
    /// </summary>
    public LicitacionDuplicadaException()
        : base("Ya existe una licitación con el mismo código.")
    {
    }
}
