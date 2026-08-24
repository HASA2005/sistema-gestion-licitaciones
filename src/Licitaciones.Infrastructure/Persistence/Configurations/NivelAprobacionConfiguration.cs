using Licitaciones.Domain.Aprobaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Licitaciones.Infrastructure.Persistence.Configurations;
internal sealed class NivelAprobacionConfiguration : IEntityTypeConfiguration<NivelAprobacion>
{ public void Configure(EntityTypeBuilder<NivelAprobacion> b) { b.ToTable("niveles_aprobacion"); b.HasKey(x => x.Id).HasName("pk_niveles_aprobacion"); b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever(); b.Property(x => x.Responsable).HasColumnName("responsable").HasMaxLength(150).IsRequired(); b.Property(x => x.MontoMinimoCrc).HasColumnName("monto_minimo_crc").HasPrecision(18, 2); b.Property(x => x.MontoMaximoCrc).HasColumnName("monto_maximo_crc").HasPrecision(18, 2); } }
