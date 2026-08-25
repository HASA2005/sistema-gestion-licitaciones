using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Aprobaciones;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Infrastructure;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
builder.Services.AddScoped<CrearLicitacionService>();
builder.Services.AddScoped<PublicarLicitacionService>();
builder.Services.AddScoped<GestionarLicitacionesService>();
builder.Services.AddScoped<RegistrarProveedorService>();
builder.Services.AddScoped<GestionarProveedoresService>();
builder.Services.AddScoped<OfertaService>();
builder.Services.AddScoped<NivelAprobacionService>();
builder.Services.AddScoped<TipoCambioService>();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Licitaciones")
        ?? throw new InvalidOperationException(
            "Debe configurar ConnectionStrings:Licitaciones."));

var app = builder.Build();

if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<LicitacionesDbContext>().Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapHealthChecks("/health");


app.Run();
