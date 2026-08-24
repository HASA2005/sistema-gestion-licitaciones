namespace Licitaciones.Application.Licitaciones;

/// <summary>
/// Indica que otra operación modificó la licitación durante el guardado.
/// </summary>
public sealed class LicitacionConcurrenciaException : Exception
{
    /// <summary>
    /// Inicializa la excepción con un mensaje que permite reintentar de forma segura.
    /// </summary>
    public LicitacionConcurrenciaException()
        : base(
            "La licitación fue modificada por otra operación. " +
            "Actualice los datos e intente nuevamente.")
    {
    }
}
