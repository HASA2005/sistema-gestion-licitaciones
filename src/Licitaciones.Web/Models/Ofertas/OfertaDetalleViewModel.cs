namespace Licitaciones.Web.Models.Ofertas;

public sealed record OfertaDetalleViewModel(
    Guid Id,
    string CodigoLicitacion,
    string TituloLicitacion,
    string NombreProveedor,
    decimal MontoCrc,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);