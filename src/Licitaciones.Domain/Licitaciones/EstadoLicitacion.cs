namespace Licitaciones.Domain.Licitaciones;

/// <summary>
/// Define los estados del ciclo de vida de una licitación.
/// </summary>
public enum EstadoLicitacion
{
    /// <summary>
    /// La licitación puede editarse y aún no está disponible para recibir ofertas.
    /// </summary>
    Borrador,

    /// <summary>
    /// La licitación está disponible para recibir ofertas.
    /// </summary>
    Publicada,

    /// <summary>
    /// La licitación finalizó y ya no admite ofertas.
    /// </summary>
    Cerrada
}
