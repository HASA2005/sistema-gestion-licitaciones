namespace Licitaciones.Application.Ofertas;
public sealed class OfertaNoEncontradaException : Exception { public OfertaNoEncontradaException() : base("La oferta no existe.") { } }
public sealed class OfertaDuplicadaException : Exception { public OfertaDuplicadaException() : base("El proveedor ya tiene una oferta para esta licitación.") { } }
public sealed class OfertaReglaException(string message) : Exception(message);
