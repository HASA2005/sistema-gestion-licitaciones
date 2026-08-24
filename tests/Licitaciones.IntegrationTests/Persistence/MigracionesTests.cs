using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Persistence;

public sealed class MigracionesTests
{
    [Fact]
    public async Task BaseVacia_AplicaMigracionInicialYPermitePersistirProveedor()
    {
        await using var postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("licitaciones")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await postgres.StartAsync();

        var opciones = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(postgres.GetConnectionString())
            .Options;

        await using var contexto = new LicitacionesDbContext(opciones);
        await contexto.Database.MigrateAsync();

        var migracionesAplicadas = await contexto.Database
            .GetAppliedMigrationsAsync();

        Assert.Contains(
            migracionesAplicadas,
            migracion => migracion.EndsWith(
                "_CrearProveedores",
                StringComparison.Ordinal));

        await contexto.Proveedores.AddAsync(new Proveedor("Empresa Central"));
        await contexto.SaveChangesAsync();

        Assert.Equal(1, await contexto.Proveedores.CountAsync());
    }
}
