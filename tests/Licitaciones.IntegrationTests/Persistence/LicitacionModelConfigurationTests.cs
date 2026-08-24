using Licitaciones.Domain.Licitaciones;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Licitaciones.IntegrationTests.Persistence;

public sealed class LicitacionModelConfigurationTests
{
    [Fact]
    public void ModeloLicitacion_ConfiguraIntegridadYConcurrencia()
    {
        var opciones = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=licitaciones_modelo;Username=test;Password=test")
            .Options;

        using var contexto = new LicitacionesDbContext(opciones);
        var modeloDiseno = contexto
            .GetService<IDesignTimeModel>()
            .Model;
        var entidad = modeloDiseno.FindEntityType(typeof(Licitacion));

        Assert.NotNull(entidad);
        Assert.Equal("licitaciones", entidad.GetTableName());
        Assert.Equal(
            nameof(Licitacion.Id),
            Assert.Single(entidad.FindPrimaryKey()!.Properties).Name);
        Assert.False(entidad.FindProperty(nameof(Licitacion.Codigo))!.IsNullable);
        Assert.Equal(
            Licitacion.LongitudMaximaCodigo,
            entidad.FindProperty(nameof(Licitacion.Codigo))!.GetMaxLength());
        Assert.Equal(
            Licitacion.LongitudMaximaCodigo,
            entidad.FindProperty(nameof(Licitacion.CodigoNormalizado))!
                .GetMaxLength());
        Assert.Equal(
            Licitacion.LongitudMaximaTitulo,
            entidad.FindProperty(nameof(Licitacion.Titulo))!.GetMaxLength());
        Assert.False(entidad.FindProperty(nameof(Licitacion.Titulo))!.IsNullable);
        Assert.False(
            entidad.FindProperty(nameof(Licitacion.FechaCierre))!.IsNullable);

        var presupuesto = entidad.FindProperty(
            nameof(Licitacion.PresupuestoEstimadoCrc));
        Assert.NotNull(presupuesto);
        Assert.Equal(18, presupuesto.GetPrecision());
        Assert.Equal(2, presupuesto.GetScale());

        var indiceCodigo = Assert.Single(
            entidad.GetIndexes(),
            indice => indice.Properties.Count == 1
                && indice.Properties[0].Name
                    == nameof(Licitacion.CodigoNormalizado));
        Assert.True(indiceCodigo.IsUnique);

        var estado = entidad.FindProperty(nameof(Licitacion.Estado));
        Assert.NotNull(estado);
        Assert.NotNull(estado.GetTypeMapping().Converter);

        var restricciones = entidad.GetCheckConstraints().ToArray();
        Assert.Contains(
            restricciones,
            restriccion => restriccion.Name
                == "ck_licitaciones_presupuesto_positivo");
        Assert.Single(entidad.GetForeignKeys());

        var version = entidad.FindProperty(nameof(Licitacion.Version));
        Assert.NotNull(version);
        Assert.True(version.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, version.ValueGenerated);
    }
}
