using Domain.Model.Dto;
using Utilities.Shared;

namespace Infrastructure.Repository.InterfacesServices
{
    public interface IProductoStockService
    {
        Task<ApiResponse<object>> RegistrarMovimientoAsync(ProductoMovimientoStock stock);
    }
}
