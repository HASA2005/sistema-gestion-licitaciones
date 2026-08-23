using Licitaciones.Domain.Proveedores;

namespace Licitaciones.UnitTests.Domain.Proveedores;

public sealed class ProveedorTests
{
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
