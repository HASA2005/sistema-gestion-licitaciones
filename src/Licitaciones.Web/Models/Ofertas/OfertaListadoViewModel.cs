namespace Licitaciones.Web.Models.Ofertas;

public sealed record OfertaListadoViewModel(
    Guid Id,
    string CodigoLicitacion,
    string NombreProveedor,
    decimal MontoCrc,
    DateTimeOffset CreatedAt);
