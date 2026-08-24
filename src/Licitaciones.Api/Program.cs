using Licitaciones.Api.Endpoints.Proveedores;
using Licitaciones.Api.Errors;
using Licitaciones.Application.Proveedores;
using Licitaciones.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.Configure<RouteHandlerOptions>(
    opciones => opciones.ThrowOnBadRequest = true);
builder.Services.AddScoped<RegistrarProveedorService>();
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("Licitaciones")
        ?? throw new InvalidOperationException(
            "Debe configurar ConnectionStrings:Licitaciones."));

var app = builder.Build();

app.UseExceptionHandler();

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
