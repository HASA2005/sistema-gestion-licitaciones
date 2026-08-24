using Licitaciones.Domain.Ofertas;

namespace Licitaciones.Application.Ofertas;

public interface IOfertaRepository
{
    Task AgregarAsync(Oferta oferta, CancellationToken ct = default);
    Task<Oferta?> ObtenerAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Oferta>> ListarAsync(Guid? licitacionId, Guid? proveedorId, CancellationToken ct = default);
    Task<bool> ExisteParaParejaAsync(Guid licitacionId, Guid proveedorId, Guid? excluirId = null, CancellationToken ct = default);
    Task GuardarAsync(CancellationToken ct = default);
    Task EliminarAsync(Oferta oferta, CancellationToken ct = default);
}
