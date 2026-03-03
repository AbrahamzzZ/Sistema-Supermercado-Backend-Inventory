namespace Domain.Model;
    public class ProductoStock
    {
        public int IdProducto { get; set; }
        public int Stock { get; set; }
        public decimal? PrecioCompra { get; set; }
        public decimal? PrecioVenta { get; set; }
    }
