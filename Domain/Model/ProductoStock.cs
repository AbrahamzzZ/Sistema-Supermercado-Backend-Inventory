using Domain.Model.Common;

namespace Domain.Model;

public class ProductoStock : EntidadAuditable
{
    public int IdProducto { get; set; }
    public int Stock { get; set; }
    public decimal? PrecioCompra { get; set; }
    public decimal? PrecioVenta { get; set; }
}
