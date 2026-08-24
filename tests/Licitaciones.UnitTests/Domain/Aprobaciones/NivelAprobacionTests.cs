using Licitaciones.Domain.Aprobaciones;
namespace Licitaciones.UnitTests.Domain.Aprobaciones;
public sealed class NivelAprobacionTests
{ [Fact] public void RangoValido_IncluyeLimites() { var n = new NivelAprobacion("Gerencia", 1000m, 9999.99m); Assert.True(n.Incluye(1000m)); Assert.True(n.Incluye(9999.99m)); } [Fact] public void MaximoMenorQueMinimo_Rechaza() => Assert.Throws<ArgumentException>(() => new NivelAprobacion("X", 100m, 99m)); [Fact] public void RangoAbierto_IncluyeMontosAltos() { var n = new NivelAprobacion("Junta", 10000000m, null); Assert.True(n.Incluye(99999999m)); } }
