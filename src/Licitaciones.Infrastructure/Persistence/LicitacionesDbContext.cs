using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Domain.Ofertas;
using Licitaciones.Domain.Aprobaciones;
using Licitaciones.Domain.TiposCambio;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Infrastructure.Persistence;

public sealed class LicitacionesDbContext : DbContext
{
    public LicitacionesDbContext(
        DbContextOptions<LicitacionesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Licitacion> Licitaciones => Set<Licitacion>();

    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    public DbSet<NivelAprobacion> NivelesAprobacion => Set<NivelAprobacion>();
    public DbSet<TipoCambio> TiposCambio => Set<TipoCambio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LicitacionesDbContext).Assembly);
    }
}
