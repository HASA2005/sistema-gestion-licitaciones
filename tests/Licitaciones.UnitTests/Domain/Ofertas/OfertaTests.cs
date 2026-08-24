using Licitaciones.Domain.Ofertas;

namespace Licitaciones.UnitTests.Domain.Ofertas;

public sealed class OfertaTests
{
    [Fact]
    public void Crear_ConMontoCero_LanzaError() =>
        Assert.Throws<ArgumentException>(() => new Oferta(Guid.NewGuid(), Guid.NewGuid(), 0m, DateTimeOffset.UtcNow));

    [Fact]
    public void Crear_ConMontoValido_ConservaDatos()
    {
        var l = Guid.NewGuid(); var p = Guid.NewGuid();
        var oferta = new Oferta(l, p, 100.50m, DateTimeOffset.UtcNow);
        Assert.Equal(l, oferta.LicitacionId); Assert.Equal(p, oferta.ProveedorId); Assert.Equal(100.50m, oferta.MontoCrc);
    }

    [Fact]
    public void Editar_ActualizaMonto()
    {
        var oferta = new Oferta(Guid.NewGuid(), Guid.NewGuid(), 100m, DateTimeOffset.UtcNow);
        oferta.Editar(90m, DateTimeOffset.UtcNow.AddMinutes(1));
        Assert.Equal(90m, oferta.MontoCrc);
    }
}
