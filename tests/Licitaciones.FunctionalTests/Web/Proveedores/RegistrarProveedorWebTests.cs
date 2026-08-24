using System.Net;
using Licitaciones.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Licitaciones.FunctionalTests.Web.Proveedores;

public sealed class RegistrarProveedorWebTests
{
    [Fact]
    public async Task Get_Registrar_MuestraFormularioConProteccionAntiforgery()
    {
        await using var aplicacion = new WebFactory();
        using var cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var respuesta = await cliente.GetAsync("/proveedores/registrar");

        var contenido = await respuesta.Content.ReadAsStringAsync();

        Assert.True(
            respuesta.StatusCode == HttpStatusCode.OK,
            $"Se esperaba 200 OK, pero se obtuvo {(int)respuesta.StatusCode}. Cuerpo: {contenido}");
        Assert.Equal(
            "text/html",
            respuesta.Content.Headers.ContentType?.MediaType);

        Assert.Contains("Registrar proveedor", contenido);
        Assert.Contains("name=\"Nombre\"", contenido);
        Assert.Contains("__RequestVerificationToken", contenido);
    }

    private sealed class WebFactory : WebApplicationFactory<WebAssemblyMarker>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
                services
                    .AddDataProtection()
                    .UseEphemeralDataProtectionProvider());
        }
    }
}
