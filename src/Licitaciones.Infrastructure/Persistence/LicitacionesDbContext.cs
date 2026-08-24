using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Domain.Ofertas;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LicitacionesDbContext).Assembly);
    }
}
