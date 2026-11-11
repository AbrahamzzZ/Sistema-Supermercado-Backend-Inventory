namespace Domain.Model.Dto
{
    public class HistorialPrecioDto
    {
        public int IdHistorialPrecio { get; set; }
        public int IdProducto { get; set; }
        public decimal? PrecioCompraAnterior { get; set; }
        public decimal? PrecioCompraNuevo { get; set; }
        public decimal? PrecioVentaAnterior { get; set; }
        public decimal? PrecioVentaNuevo { get; set; }
        public string? Referencia { get; set; }
        public int? IdUsuarioCreacion { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
