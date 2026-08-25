namespace Licitaciones.Application.TiposCambio;

public sealed class TipoCambioActivoException : InvalidOperationException
{
    public TipoCambioActivoException()
        : base("Solo puede existir un tipo de cambio activo.")
    {
    }
}
