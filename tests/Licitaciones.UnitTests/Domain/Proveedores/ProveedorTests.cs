using Licitaciones.Domain.Proveedores;

namespace Licitaciones.UnitTests.Domain.Proveedores;

public sealed class ProveedorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_ConNombreVacio_LanzaErrorControlado(string? nombre)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Proveedor(nombre!));

        Assert.Equal("nombre", excepcion.ParamName);
        Assert.Contains("El nombre del proveedor es obligatorio.", excepcion.Message);
    }

    [Theory]
    [InlineData("Empresa Central")]
    [InlineData(" empresa central ")]
    [InlineData("EMPRESA   CENTRAL")]
    public void Crear_ConNombresEquivalentes_GeneraMismaNormalizacion(string nombre)
    {
        var proveedor = new Proveedor(nombre);

        Assert.Equal("EMPRESA CENTRAL", proveedor.NombreNormalizado);
    }
}
