namespace Domain.Model.Dto
{
    public class ProductoStockDto
    {
        public int IdProducto { get; set; }
        public int Stock { get; set; }
        public decimal? PrecioCompra { get; set; }
        public decimal? PrecioVenta { get; set; }
    }
}
