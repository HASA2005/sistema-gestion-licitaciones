namespace Licitaciones.Domain.Ofertas;

public sealed class Oferta
{
    private Oferta() { }

    public Oferta(Guid licitacionId, Guid proveedorId, decimal montoCrc, DateTimeOffset registradaAt)
    {
        ValidarMonto(montoCrc);
        Id = Guid.NewGuid(); LicitacionId = licitacionId; ProveedorId = proveedorId;
        MontoCrc = montoCrc; CreatedAt = registradaAt.ToUniversalTime(); UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }
    public Guid LicitacionId { get; private set; }
    public Guid ProveedorId { get; private set; }
    public decimal MontoCrc { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Editar(decimal montoCrc, DateTimeOffset ahora)
    {
        ValidarMonto(montoCrc); MontoCrc = montoCrc; UpdatedAt = ahora.ToUniversalTime();
    }

    private static void ValidarMonto(decimal montoCrc)
    {
        if (montoCrc <= 0) throw new ArgumentException("El monto ofertado debe ser mayor que cero.", nameof(montoCrc));
        if (decimal.Round(montoCrc, 2) != montoCrc) throw new ArgumentException("El monto ofertado no puede tener más de dos decimales.", nameof(montoCrc));
    }
}
