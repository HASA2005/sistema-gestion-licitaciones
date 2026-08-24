using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Licitaciones.IntegrationTests.Persistence;

public sealed class ProveedorModelConfigurationTests
{
    [Fact]
    public void ModeloProveedor_ConfiguraIntegridadYConcurrencia()
    {
        var opciones = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=licitaciones_modelo;Username=test;Password=test")
            .Options;

        using var contexto = new LicitacionesDbContext(opciones);
        var entidad = contexto.Model.FindEntityType(typeof(Proveedor));

        Assert.NotNull(entidad);
        Assert.Equal("proveedores", entidad.GetTableName());
        Assert.Equal(
            nameof(Proveedor.Id),
            Assert.Single(entidad.FindPrimaryKey()!.Properties).Name);
        Assert.False(entidad.FindProperty(nameof(Proveedor.Nombre))!.IsNullable);
        Assert.False(
            entidad.FindProperty(nameof(Proveedor.NombreNormalizado))!.IsNullable);

        var indiceNombre = Assert.Single(
            entidad.GetIndexes(),
            indice => indice.Properties.Count == 1
                && indice.Properties[0].Name == nameof(Proveedor.NombreNormalizado));
        Assert.True(indiceNombre.IsUnique);

        var version = entidad.FindProperty(nameof(Proveedor.Version));
        Assert.NotNull(version);
        Assert.True(version.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);
    }
}
