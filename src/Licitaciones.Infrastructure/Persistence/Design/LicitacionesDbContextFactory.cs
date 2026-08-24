using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Licitaciones.Infrastructure.Persistence.Design;

public sealed class LicitacionesDbContextFactory
    : IDesignTimeDbContextFactory<LicitacionesDbContext>
{
    public LicitacionesDbContext CreateDbContext(string[] args)
    {
        var cadenaConexion = Environment.GetEnvironmentVariable(
                "ConnectionStrings__Licitaciones")
            ?? "Host=localhost;Database=licitaciones;Username=postgres";

        var opciones = new DbContextOptionsBuilder<LicitacionesDbContext>()
            .UseNpgsql(cadenaConexion)
            .Options;

        return new LicitacionesDbContext(opciones);
    }
}
