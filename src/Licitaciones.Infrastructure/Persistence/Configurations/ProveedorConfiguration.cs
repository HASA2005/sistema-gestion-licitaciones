using Licitaciones.Domain.Proveedores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    internal const string IndiceNombreNormalizado =
        "ux_proveedores_nombre_normalizado";

    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores");

        builder.HasKey(proveedor => proveedor.Id)
            .HasName("pk_proveedores");

        builder.Property(proveedor => proveedor.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(proveedor => proveedor.Nombre)
            .HasColumnName("nombre")
            .IsRequired();

        builder.Property(proveedor => proveedor.NombreNormalizado)
            .HasColumnName("nombre_normalizado")
            .IsRequired();

        builder.HasIndex(proveedor => proveedor.NombreNormalizado)
            .IsUnique()
            .HasDatabaseName(IndiceNombreNormalizado);

        builder.Property(proveedor => proveedor.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(proveedor => proveedor.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(proveedor => proveedor.Version)
            .HasColumnName("xmin")
            .IsRowVersion();
    }
}
