using Licitaciones.Api.Endpoints.Proveedores;
using Licitaciones.Application.Proveedores;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddScoped<RegistrarProveedorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapRegistrarProveedor();

app.Run();

public partial class Program
{
}
