using Licitaciones.Domain.Proveedores;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Licitaciones.IntegrationTests.Persistence;

public sealed class ProveedorRepositoryTests
{
    [Fact]
    public async Task AgregarYConsultarAsync_PersisteProveedorEnPostgreSql()
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

        await using (var contexto = new LicitacionesDbContext(opciones))
        {
            await contexto.Database.EnsureCreatedAsync();

            var repositorio = new ProveedorRepository(contexto);
            await repositorio.AgregarAsync(new Proveedor("Empresa Central"));
        }

        await using var contextoVerificacion = new LicitacionesDbContext(opciones);
        var repositorioVerificacion = new ProveedorRepository(contextoVerificacion);

        var existe = await repositorioVerificacion
            .ExisteConNombreNormalizadoAsync("EMPRESA CENTRAL");
        var proveedorGuardado = await contextoVerificacion.Proveedores
            .AsNoTracking()
            .SingleAsync();

        Assert.True(existe);
        Assert.Equal("Empresa Central", proveedorGuardado.Nombre);
        Assert.NotEqual(0u, proveedorGuardado.Version);
    }
}
