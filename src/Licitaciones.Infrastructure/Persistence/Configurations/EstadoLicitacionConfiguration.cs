using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class EstadoLicitacionConfiguration
    : IEntityTypeConfiguration<EstadoLicitacionRegistro>
{
    public void Configure(EntityTypeBuilder<EstadoLicitacionRegistro> builder)
    {
        builder.ToTable("estados_licitacion");

        builder.HasKey(estado => estado.Codigo)
            .HasName("pk_estados_licitacion");

        builder.Property(estado => estado.Codigo)
            .HasColumnName("codigo")
            .HasConversion<string>()
            .HasMaxLength(20)
            .ValueGeneratedNever();

        builder.Property(estado => estado.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasData(
            new
            {
                Codigo = EstadoLicitacion.Borrador,
                Nombre = "Borrador"
            },
            new
            {
                Codigo = EstadoLicitacion.Publicada,
                Nombre = "Publicada"
            },
            new
            {
                Codigo = EstadoLicitacion.Cerrada,
                Nombre = "Cerrada"
            });
    }
}
