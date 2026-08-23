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

    [Fact]
    public void Crear_ConRepresentacionesUnicodeEquivalentes_GeneraMismaNormalizacion()
    {
        var nombreCompuesto = new Proveedor("Café Central");
        var nombreDescompuesto = new Proveedor("Cafe\u0301 Central");

        Assert.Equal(
            nombreCompuesto.NombreNormalizado,
            nombreDescompuesto.NombreNormalizado);
        Assert.Equal("CAFÉ CENTRAL", nombreDescompuesto.NombreNormalizado);
    }

    [Theory]
    [InlineData("Empresa 123", "EMPRESA 123")]
    [InlineData("Servicios S.A.", "SERVICIOS S.A.")]
    [InlineData("Comercial, Regional", "COMERCIAL, REGIONAL")]
    [InlineData("Proveedor (Central)", "PROVEEDOR (CENTRAL)")]
    public void Crear_ConCaracteresPermitidos_ConservaNombreValido(
        string nombre,
        string nombreNormalizadoEsperado)
    {
        var proveedor = new Proveedor(nombre);

        Assert.Equal(nombreNormalizadoEsperado, proveedor.NombreNormalizado);
    }

    [Theory]
    [InlineData("Empresa @ Central")]
    [InlineData("Proveedor/Regional")]
    [InlineData("Compañía #1")]
    [InlineData("Servicios & Asociados")]
    public void Crear_ConCaracteresNoPermitidos_LanzaErrorControlado(string nombre)
    {
        var excepcion = Assert.Throws<ArgumentException>(
            () => new Proveedor(nombre));

        Assert.Equal("nombre", excepcion.ParamName);
        Assert.Contains(
            "El nombre del proveedor contiene caracteres no permitidos.",
            excepcion.Message);
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
