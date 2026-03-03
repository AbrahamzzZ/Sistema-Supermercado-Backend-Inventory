using Domain.Model.Dto;

namespace Infrastructure.Repository.InterfacesRepository
{
    public interface IProductoStockRepository
    {
        Task<int> RegistrarMovimientoAsync(ProductoMovimientoStock stock);
    }
}
