using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface IProductoStockConsultaService
    {
        Task<ApiResponse<ProductoStockDto>> ObtenerStockAsync(int idProducto);
        Task<ApiResponse<List<HistorialPrecioDto>>> ListarHistorialPreciosAsync(int idProducto);
    }
}
