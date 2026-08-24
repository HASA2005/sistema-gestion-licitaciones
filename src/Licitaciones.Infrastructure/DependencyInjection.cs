using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Licitaciones.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string cadenaConexion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);

        services.AddDbContext<LicitacionesDbContext>(
            opciones => opciones.UseNpgsql(cadenaConexion));
        services.AddScoped<ILicitacionRepository, LicitacionRepository>();
        services.AddScoped<IProveedorRepository, ProveedorRepository>();

        return services;
    }
}
