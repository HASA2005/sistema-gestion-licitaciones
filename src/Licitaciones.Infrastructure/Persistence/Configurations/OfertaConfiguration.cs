using Licitaciones.Domain.Ofertas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;
internal sealed class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
{
    public void Configure(EntityTypeBuilder<Oferta> b)
    {
        b.ToTable("ofertas", t => t.HasCheckConstraint("ck_ofertas_monto_positivo", "monto_crc > 0"));
        b.HasKey(x => x.Id).HasName("pk_ofertas");
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.LicitacionId).HasColumnName("licitacion_id").IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        b.Property(x => x.MontoCrc).HasColumnName("monto_crc").HasPrecision(18, 2).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamp with time zone");
        b.HasIndex(x => new { x.LicitacionId, x.ProveedorId }).IsUnique().HasDatabaseName("ux_ofertas_licitacion_proveedor");
        b.HasOne<Licitaciones.Domain.Licitaciones.Licitacion>().WithMany().HasForeignKey(x => x.LicitacionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Licitaciones.Domain.Proveedores.Proveedor>().WithMany().HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Restrict);
    }
}
