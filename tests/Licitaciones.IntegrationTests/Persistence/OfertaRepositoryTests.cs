using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Persistence;

public sealed class OfertaRepositoryTests
{
    [Fact]
    public async Task Oferta_PersisteYRespetaIndiceUnico()
    {
        await using var pg = new PostgreSqlBuilder("postgres:16-alpine").WithDatabase("licitaciones").WithUsername("postgres").WithPassword("postgres").Build();
        await pg.StartAsync();
        var options = new DbContextOptionsBuilder<LicitacionesDbContext>().UseNpgsql(pg.GetConnectionString()).Options;
        await using var db = new LicitacionesDbContext(options); await db.Database.MigrateAsync();
        var proveedor = new Proveedor("Proveedor Oferta"); var licitacion = new Licitacion("OF-001", "Compra", 1000m, DateTimeOffset.UtcNow.AddDays(1)); licitacion.Publicar(DateTimeOffset.UtcNow);
        db.Proveedores.Add(proveedor); db.Licitaciones.Add(licitacion); await db.SaveChangesAsync();
        var repo = new OfertaRepository(db); await repo.AgregarAsync(new Oferta(licitacion.Id, proveedor.Id, 900m, DateTimeOffset.UtcNow));
        Assert.Single(await db.Ofertas.ToListAsync());
        await Assert.ThrowsAsync<Licitaciones.Application.Ofertas.OfertaDuplicadaException>(() => repo.AgregarAsync(new Oferta(licitacion.Id, proveedor.Id, 800m, DateTimeOffset.UtcNow)));
    }
}
