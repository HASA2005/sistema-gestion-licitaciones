using System.ComponentModel.DataAnnotations;
namespace Licitaciones.Web.Models.TiposCambio; public sealed class TipoCambioViewModel { [Range(typeof(decimal), "0.01", "9999999999999999")] public decimal CrcPorUsd { get; set; } public bool Activo { get; set; } }
