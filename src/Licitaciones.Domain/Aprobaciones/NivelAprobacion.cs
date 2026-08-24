namespace Licitaciones.Domain.Aprobaciones;

public sealed class NivelAprobacion
{
    private NivelAprobacion() { Responsable = string.Empty; }
    public NivelAprobacion(string responsable, decimal montoMinimoCrc, decimal? montoMaximoCrc)
    {
        if (string.IsNullOrWhiteSpace(responsable)) throw new ArgumentException("El responsable es obligatorio.", nameof(responsable));
        ValidarMontos(montoMinimoCrc, montoMaximoCrc);
        Id = Guid.NewGuid(); Responsable = responsable.Trim(); MontoMinimoCrc = montoMinimoCrc; MontoMaximoCrc = montoMaximoCrc;
    }
    public Guid Id { get; private set; }
    public string Responsable { get; private set; }
    public decimal MontoMinimoCrc { get; private set; }
    public decimal? MontoMaximoCrc { get; private set; }
    public void Editar(string responsable, decimal minimo, decimal? maximo) { if (string.IsNullOrWhiteSpace(responsable)) throw new ArgumentException("El responsable es obligatorio."); ValidarMontos(minimo, maximo); Responsable = responsable.Trim(); MontoMinimoCrc = minimo; MontoMaximoCrc = maximo; }
    public bool Incluye(decimal monto) => monto >= MontoMinimoCrc && (!MontoMaximoCrc.HasValue || monto <= MontoMaximoCrc.Value);
    private static void ValidarMontos(decimal min, decimal? max) { if (min <= 0) throw new ArgumentException("El monto mínimo debe ser mayor que cero."); if (decimal.Round(min, 2) != min || max.HasValue && decimal.Round(max.Value, 2) != max.Value) throw new ArgumentException("Los montos no pueden tener más de dos decimales."); if (max.HasValue && max.Value < min) throw new ArgumentException("El monto máximo debe ser mayor o igual al mínimo."); }
}
