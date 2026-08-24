using Licitaciones.Domain.Licitaciones;

namespace Licitaciones.Infrastructure.Persistence.Models;

internal sealed class EstadoLicitacionRegistro
{
    private EstadoLicitacionRegistro()
    {
        Nombre = string.Empty;
    }

    public EstadoLicitacion Codigo { get; private set; }

    public string Nombre { get; private set; }
}
