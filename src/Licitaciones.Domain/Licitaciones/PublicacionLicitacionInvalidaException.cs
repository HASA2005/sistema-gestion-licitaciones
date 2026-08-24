namespace Licitaciones.Domain.Licitaciones;

/// <summary>
/// Representa el incumplimiento de una regla de negocio al publicar una licitación.
/// </summary>
public sealed class PublicacionLicitacionInvalidaException : InvalidOperationException
{
    /// <summary>
    /// Inicializa la excepción con un mensaje seguro para el usuario.
    /// </summary>
    /// <param name="motivo">Regla de publicación incumplida.</param>
    /// <param name="mensaje">Descripción segura de la regla incumplida.</param>
    public PublicacionLicitacionInvalidaException(
        MotivoPublicacionInvalida motivo,
        string mensaje)
        : base(mensaje)
    {
        Motivo = motivo;
    }

    /// <summary>
    /// Obtiene la regla que impidió la publicación.
    /// </summary>
    public MotivoPublicacionInvalida Motivo { get; }
}
