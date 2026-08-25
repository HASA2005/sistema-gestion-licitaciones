namespace Licitaciones.Application.Licitaciones;

public sealed class LicitacionConOfertasException : InvalidOperationException
{
    public LicitacionConOfertasException()
        : base("No se puede eliminar una licitación con ofertas.")
    {
    }
}
