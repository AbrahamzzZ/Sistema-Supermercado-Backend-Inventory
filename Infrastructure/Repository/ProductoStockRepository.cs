using Domain.Context;
using Domain.Model;
using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class ProductoStockRepository : IProductoStockRepository
    {
        private readonly InventorySistemaSupermercadoContext _context;

        public ProductoStockRepository(InventorySistemaSupermercadoContext context)
        {
            _context = context;
        }

        public async Task<int> RegistrarMovimientoAsync(ProductoMovimientoStock stock)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_MOVIMIENTO_STOCK @Id_Producto, @Tipo, @Cantidad, @Precio_Compra, @Precio_Venta, @Referencia",
                new SqlParameter("@Id_Producto", stock.IdProducto),
                new SqlParameter("@Tipo", stock.Tipo ?? (object)DBNull.Value),
                new SqlParameter("@Cantidad", stock.Cantidad),
                new SqlParameter("@Precio_Compra", stock.PrecioCompra),
                new SqlParameter("@Precio_Venta", stock.PrecioVenta),
                 new SqlParameter("@Referencia", stock.Referencia ?? (object)DBNull.Value)
            );
        }
    }
}
