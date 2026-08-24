using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Aprobaciones;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CrearLicitacionService>();
builder.Services.AddScoped<PublicarLicitacionService>();
builder.Services.AddScoped<RegistrarProveedorService>();
builder.Services.AddScoped<OfertaService>();
builder.Services.AddScoped<NivelAprobacionService>();
builder.Services.AddScoped<TipoCambioService>();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Licitaciones")
        ?? throw new InvalidOperationException(
            "Debe configurar ConnectionStrings:Licitaciones."));

var app = builder.Build();

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


app.Run();
