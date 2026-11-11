using Domain.Model.Dto;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface IProductoStockRepository
    {
        Task<int> RegistrarMovimientoAsync(ProductoMovimientoStock stock, int? idUsuario);

        Task<ProductoStockDto?> ObtenerStockAsync(int idProducto);

        Task<List<HistorialPrecioDto>> ListarHistorialPreciosAsync(int idProducto);
    }
}
