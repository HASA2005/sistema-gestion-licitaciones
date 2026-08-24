using Licitaciones.Application.Proveedores;
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

    [Fact]
    public async Task AgregarAsync_EnCondicionDeCarreraDuplicada_LanzaErrorControlado()
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

        await using (var contextoInicial = new LicitacionesDbContext(opciones))
        {
            await contextoInicial.Database.EnsureCreatedAsync();
        }

        await using var primerContexto = new LicitacionesDbContext(opciones);
        await using var segundoContexto = new LicitacionesDbContext(opciones);
        var primerRepositorio = new ProveedorRepository(primerContexto);
        var segundoRepositorio = new ProveedorRepository(segundoContexto);

        Assert.False(await primerRepositorio
            .ExisteConNombreNormalizadoAsync("EMPRESA CENTRAL"));
        Assert.False(await segundoRepositorio
            .ExisteConNombreNormalizadoAsync("EMPRESA CENTRAL"));

        await primerRepositorio.AgregarAsync(new Proveedor("Empresa Central"));

        var excepcion = await Assert.ThrowsAsync<ProveedorDuplicadoException>(
            () => segundoRepositorio.AgregarAsync(
                new Proveedor(" empresa   central ")));

        Assert.Equal(
            "Ya existe un proveedor con el mismo nombre.",
            excepcion.Message);

        await using var contextoVerificacion = new LicitacionesDbContext(opciones);
        Assert.Equal(1, await contextoVerificacion.Proveedores.CountAsync());
    }
}
