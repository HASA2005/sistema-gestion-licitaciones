using Licitaciones.Domain.Licitaciones;
using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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

    [Fact]
    public async Task BaseConProveedores_AplicaMigracionDeLicitacionesSinPerderDatos()
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
        var migrador = contexto.GetService<IMigrator>();

        await migrador.MigrateAsync("20260824003850_CrearProveedores");
        await contexto.Proveedores.AddAsync(new Proveedor("Empresa Central"));
        await contexto.SaveChangesAsync();

        await migrador.MigrateAsync();

        var migracionesAplicadas = await contexto.Database
            .GetAppliedMigrationsAsync();

        Assert.Contains(
            migracionesAplicadas,
            migracion => migracion.EndsWith(
                "_CrearLicitaciones",
                StringComparison.Ordinal));
        Assert.Equal(1, await contexto.Proveedores.CountAsync());

        await contexto.Licitaciones.AddAsync(new Licitacion(
            "LIC-2026-001",
            "Compra de equipo",
            1_250_000m,
            new DateTimeOffset(2026, 9, 30, 22, 0, 0, TimeSpan.Zero)));
        await contexto.SaveChangesAsync();

        Assert.Equal(1, await contexto.Licitaciones.CountAsync());
    }
}
