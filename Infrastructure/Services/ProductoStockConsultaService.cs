using Domain.Model.Dto;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class ProductoStockConsultaService : IProductoStockConsultaService
    {
        private readonly ProductoStockRepository _productoStockRepository;

        public ProductoStockConsultaService(ProductoStockRepository productoStockRepository)
        {
            _productoStockRepository = productoStockRepository;
        }

        public async Task<ApiResponse<List<ProductoStockDto>>> ListarStockAsync()
        {
            var lista = await _productoStockRepository.ListarStockAsync();
            return new ApiResponse<List<ProductoStockDto>>
            {
                IsSuccess = true,
                Message = lista.Count == 0 ? Mensajes.MESSAGE_QUERY_EMPTY : Mensajes.MESSAGE_QUERY,
                Data = lista
            };
        }

        public async Task<ApiResponse<ProductoStockDto>> ObtenerStockAsync(int idProducto)
        {
            var stock = await _productoStockRepository.ObtenerStockAsync(idProducto);

            if (stock is null)
                return new ApiResponse<ProductoStockDto> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_NOT_FOUND };

            return new ApiResponse<ProductoStockDto> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = stock };
        }

        public async Task<ApiResponse<List<HistorialPrecioDto>>> ListarHistorialPreciosAsync(int idProducto)
        {
            var historial = await _productoStockRepository.ListarHistorialPreciosAsync(idProducto);

            if (historial.Count == 0)
                return new ApiResponse<List<HistorialPrecioDto>> { IsSuccess = false, Message = Mensajes.MESSAGE_QUERY_EMPTY, Data = historial };

            return new ApiResponse<List<HistorialPrecioDto>> { IsSuccess = true, Message = Mensajes.MESSAGE_QUERY, Data = historial };
        }
    }
}
