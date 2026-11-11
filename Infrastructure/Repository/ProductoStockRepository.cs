using Domain.Context;
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

        public async Task<int> RegistrarMovimientoAsync(ProductoMovimientoStock stock, int? idUsuario)
        {
            return await _context.Database.ExecuteSqlRawAsync(
                "EXEC PA_REGISTRAR_MOVIMIENTO_STOCK @Id_Producto, @Tipo, @Cantidad, @Precio_Compra, @Precio_Venta, @Referencia, @Id_Usuario",
                new SqlParameter("@Id_Producto", stock.IdProducto),
                new SqlParameter("@Tipo", stock.Tipo ?? (object)DBNull.Value),
                new SqlParameter("@Cantidad", stock.Cantidad),
                new SqlParameter("@Precio_Compra", stock.PrecioCompra ?? (object)DBNull.Value),
                new SqlParameter("@Precio_Venta", stock.PrecioVenta ?? (object)DBNull.Value),
                new SqlParameter("@Referencia", stock.Referencia ?? (object)DBNull.Value),
                new SqlParameter("@Id_Usuario", idUsuario ?? (object)DBNull.Value)
            );
        }

        public async Task<ProductoStockDto?> ObtenerStockAsync(int idProducto)
        {
            return await _context.ProductoStocks
                .AsNoTracking()
                .Where(p => p.IdProducto == idProducto)
                .Select(p => new ProductoStockDto
                {
                    IdProducto = p.IdProducto,
                    Stock = p.Stock,
                    PrecioCompra = p.PrecioCompra,
                    PrecioVenta = p.PrecioVenta
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<HistorialPrecioDto>> ListarHistorialPreciosAsync(int idProducto)
        {
            return await _context.HistorialPrecios
                .AsNoTracking()
                .Where(h => h.IdProducto == idProducto)
                .OrderByDescending(h => h.FechaCreacion)
                .ThenByDescending(h => h.IdHistorialPrecio)
                .Select(h => new HistorialPrecioDto
                {
                    IdHistorialPrecio = h.IdHistorialPrecio,
                    IdProducto = h.IdProducto,
                    PrecioCompraAnterior = h.PrecioCompraAnterior,
                    PrecioCompraNuevo = h.PrecioCompraNuevo,
                    PrecioVentaAnterior = h.PrecioVentaAnterior,
                    PrecioVentaNuevo = h.PrecioVentaNuevo,
                    Referencia = h.Referencia,
                    IdUsuarioCreacion = h.IdUsuarioCreacion,
                    FechaCreacion = h.FechaCreacion
                })
                .ToListAsync();
        }
    }
}
