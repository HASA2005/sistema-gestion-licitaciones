using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class LicitacionConfiguration
    : IEntityTypeConfiguration<Licitacion>
{
    internal const string IndiceCodigoNormalizado =
        "ux_licitaciones_codigo_normalizado";

    internal const string RestriccionPresupuestoPositivo =
        "ck_licitaciones_presupuesto_positivo";

    public void Configure(EntityTypeBuilder<Licitacion> builder)
    {
        builder.ToTable(
            "licitaciones",
            tabla =>
            {
                tabla.HasCheckConstraint(
                    RestriccionPresupuestoPositivo,
                    "presupuesto_estimado_crc > 0");
            });

        builder.HasKey(licitacion => licitacion.Id)
            .HasName("pk_licitaciones");

        builder.Property(licitacion => licitacion.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(licitacion => licitacion.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(Licitacion.LongitudMaximaCodigo)
            .IsRequired();

        builder.Property(licitacion => licitacion.CodigoNormalizado)
            .HasColumnName("codigo_normalizado")
            .HasMaxLength(Licitacion.LongitudMaximaCodigo)
            .IsRequired();

        builder.HasIndex(licitacion => licitacion.CodigoNormalizado)
            .IsUnique()
            .HasDatabaseName(IndiceCodigoNormalizado);

        builder.Property(licitacion => licitacion.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(Licitacion.LongitudMaximaTitulo)
            .IsRequired();

        builder.Property(licitacion => licitacion.PresupuestoEstimadoCrc)
            .HasColumnName("presupuesto_estimado_crc")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(licitacion => licitacion.FechaCierre)
            .HasColumnName("fecha_cierre")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(licitacion => licitacion.Estado)
            .HasColumnName("estado")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne<EstadoLicitacionRegistro>()
            .WithMany()
            .HasForeignKey(licitacion => licitacion.Estado)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_licitaciones_estados_licitacion_estado");

        builder.HasIndex(licitacion => licitacion.Estado)
            .HasDatabaseName("ix_licitaciones_estado");

        builder.Property(licitacion => licitacion.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(licitacion => licitacion.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(licitacion => licitacion.Version)
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
