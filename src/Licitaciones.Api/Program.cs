using Licitaciones.Api.Endpoints.Licitaciones;
using Licitaciones.Api.Endpoints.Proveedores;
using Licitaciones.Api.Endpoints.Ofertas;
using Licitaciones.Api.Endpoints.Aprobaciones;
using Licitaciones.Api.Endpoints.TiposCambio;
using Licitaciones.Api.Endpoints.Crud;
using Licitaciones.Api.Errors;
using Licitaciones.Application.Licitaciones;
using Licitaciones.Application.Proveedores;
using Licitaciones.Application.Ofertas;
using Licitaciones.Application.Aprobaciones;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.Configure<RouteHandlerOptions>(
    opciones => opciones.ThrowOnBadRequest = true);
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

app.UseExceptionHandler();
app.UseStatusCodePages(async contextoEstado =>
{
    var contextoHttp = contextoEstado.HttpContext;
    if (contextoHttp.Response.StatusCode !=
        StatusCodes.Status415UnsupportedMediaType)
    {
        return;
    }

    var problema = new ProblemDetails
    {
        Title = "Tipo de contenido no compatible.",
        Status = StatusCodes.Status415UnsupportedMediaType,
        Detail = "La solicitud debe usar Content-Type application/json."
    };
    problema.Extensions["errorCode"] = "tipo_contenido_no_compatible";
    problema.Extensions["correlationId"] = contextoHttp.TraceIdentifier;

    await contextoHttp.Response.WriteAsJsonAsync(
        problema,
        options: null,
        contentType: "application/problem+json",
        cancellationToken: contextoHttp.RequestAborted);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapCrearLicitacion();
app.MapPublicarLicitacion();
app.MapRegistrarProveedor();
app.MapOfertas();
app.MapNivelesAprobacion();
app.MapTiposCambio();
app.MapCrud();

app.Run();

public partial class Program
{
}
