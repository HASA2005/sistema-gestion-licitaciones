namespace Licitaciones.Application.Aprobaciones;

public sealed class RangoAprobacionTraslapadoException : InvalidOperationException
{
    public RangoAprobacionTraslapadoException()
        : base("El rango se traslapa con otro nivel.")
    {
    }
}
