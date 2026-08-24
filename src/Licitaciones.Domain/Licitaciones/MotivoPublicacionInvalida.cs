namespace Licitaciones.Domain.Licitaciones;

/// <summary>
/// Identifica la regla que impide publicar una licitación.
/// </summary>
public enum MotivoPublicacionInvalida
{
    /// <summary>
    /// La licitación no se encuentra en Borrador.
    /// </summary>
    Estado,

    /// <summary>
    /// Los datos obligatorios no son válidos.
    /// </summary>
    Datos,

    /// <summary>
    /// La fecha de cierre no es estrictamente futura.
    /// </summary>
    FechaCierre
}
