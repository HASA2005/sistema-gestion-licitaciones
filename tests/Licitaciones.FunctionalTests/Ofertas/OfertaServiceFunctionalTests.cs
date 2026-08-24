using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Proveedores;
using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;

namespace Licitaciones.FunctionalTests.Ofertas;

public sealed class OfertaServiceFunctionalTests
{
    [Fact]
    public async Task CrearOfertaValida_YDuplicadaEsRechazada()
    {
        var l = new Licitacion("F-1", "Compra", 100m, DateTimeOffset.UtcNow.AddDays(1)); l.Publicar(DateTimeOffset.UtcNow);
        var p = new Proveedor("Proveedor Funcional"); var repo = new OfertasRepo();
        var service = new OfertaService(repo, new LicitacionesRepo(l), new ProveedoresRepo(p));
        var creada = await service.CrearAsync(l.Id, p.Id, 90m);
        Assert.Equal(90m, creada.MontoCrc);
        await Assert.ThrowsAsync<OfertaDuplicadaException>(() => service.CrearAsync(l.Id, p.Id, 80m));
    }

    private sealed class LicitacionesRepo(Licitacion l) : ILicitacionRepository
    { public Task<bool> ExisteConCodigoNormalizadoAsync(string c, CancellationToken t = default) => Task.FromResult(false); public Task AgregarAsync(Licitacion x, CancellationToken t = default) => Task.CompletedTask; public Task<Licitacion?> ObtenerPorIdAsync(Guid id, CancellationToken t = default) => Task.FromResult<Licitacion?>(id == l.Id ? l : null); public Task GuardarCambiosAsync(Licitacion x, CancellationToken t = default) => Task.CompletedTask; }
    private sealed class ProveedoresRepo(Proveedor p) : IProveedorRepository
    { public Task<Proveedor?> ObtenerPorIdAsync(Guid id, CancellationToken t = default) => Task.FromResult<Proveedor?>(id == p.Id ? p : null); public Task<bool> ExisteConNombreNormalizadoAsync(string n, CancellationToken t = default) => Task.FromResult(false); public Task AgregarAsync(Proveedor x, CancellationToken t = default) => Task.CompletedTask; }
    private sealed class OfertasRepo : IOfertaRepository
    { private readonly List<Oferta> items = []; public Task AgregarAsync(Oferta x, CancellationToken t = default) { items.Add(x); return Task.CompletedTask; } public Task<Oferta?> ObtenerAsync(Guid id, CancellationToken t = default) => Task.FromResult(items.SingleOrDefault(x => x.Id == id)); public Task<IReadOnlyList<Oferta>> ListarAsync(Guid? l, Guid? p, CancellationToken t = default) => Task.FromResult<IReadOnlyList<Oferta>>(items.Where(x => (!l.HasValue || x.LicitacionId == l) && (!p.HasValue || x.ProveedorId == p)).ToList()); public Task<bool> ExisteParaParejaAsync(Guid l, Guid p, Guid? e = null, CancellationToken t = default) => Task.FromResult(items.Any(x => x.LicitacionId == l && x.ProveedorId == p && x.Id != e)); public Task GuardarAsync(CancellationToken t = default) => Task.CompletedTask; public Task EliminarAsync(Oferta x, CancellationToken t = default) { items.Remove(x); return Task.CompletedTask; } }
}
