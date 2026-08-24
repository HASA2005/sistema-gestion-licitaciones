namespace Licitaciones.Domain.TiposCambio;
public sealed class TipoCambio
{
    private TipoCambio() { }
    public TipoCambio(decimal crcPorUsd, DateTimeOffset creado) { Validar(crcPorUsd); Id = Guid.NewGuid(); CrcPorUsd = crcPorUsd; Activo = false; CreatedAt = creado.ToUniversalTime(); UpdatedAt = CreatedAt; }
    public Guid Id { get; private set; }
    public decimal CrcPorUsd { get; private set; }
    public bool Activo { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public void Editar(decimal valor, DateTimeOffset ahora) { Validar(valor); CrcPorUsd = valor; UpdatedAt = ahora.ToUniversalTime(); }
    public void Activar(DateTimeOffset ahora) { Activo = true; UpdatedAt = ahora.ToUniversalTime(); }
    public void Desactivar(DateTimeOffset ahora) { Activo = false; UpdatedAt = ahora.ToUniversalTime(); }
    private static void Validar(decimal v) { if (v <= 0) throw new ArgumentException("El tipo de cambio debe ser mayor que cero.", nameof(v)); if (decimal.Round(v, 2) != v) throw new ArgumentException("El tipo de cambio no puede tener más de dos decimales.", nameof(v)); }
}
