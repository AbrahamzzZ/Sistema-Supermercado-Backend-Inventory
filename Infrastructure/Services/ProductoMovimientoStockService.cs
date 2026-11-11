using Domain.Model.Dto;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Security;
using Infrastructure.Services.Interface;
using Microsoft.Extensions.Logging;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class ProductoMovimientoStockService : IProductoStockService
    {
        private readonly ProductoStockRepository _productoStockRepository;
        private readonly IValidator<ProductoMovimientoStock> _validator;
        private readonly IMasterData _masterDataValidator;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<ProductoMovimientoStockService> _logger;

        public ProductoMovimientoStockService(ProductoStockRepository productoStockRepository, IValidator<ProductoMovimientoStock> validator,
            IMasterData masterDataValidator, ICurrentUserService currentUser, ILogger<ProductoMovimientoStockService> logger)
        {
            _productoStockRepository = productoStockRepository;
            _validator = validator;
            _masterDataValidator = masterDataValidator;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> RegistrarMovimientoAsync(ProductoMovimientoStock stock)
        {
            if (stock == null)
                return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_NULL };

            var validationResult = await _validator.ValidateAsync(stock);
            if (!validationResult.IsValid)
            {
                var errores = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Movimiento de stock rechazado por validación: {Errores}", errores);
                return new ApiResponse<object> { IsSuccess = false, Message = errores };
            }

            if (!await _masterDataValidator.ProductoExisteAsync(stock.IdProducto))
            {
                _logger.LogWarning("Movimiento de stock rechazado: el producto {IdProducto} no existe en Admin", stock.IdProducto);
                return new ApiResponse<object> { IsSuccess = false, Message = "El producto no existe" };
            }

            try
            {
                await _productoStockRepository.RegistrarMovimientoAsync(stock, _currentUser.IdUsuario);
            }
            catch (Exception ex) when (ErroresNegocioSql.EsErrorDeNegocio(ex, out var sqlEx))
            {
                _logger.LogWarning("Movimiento {Tipo} rechazado para el producto {IdProducto} (cantidad {Cantidad}): {Motivo}",
                    stock.Tipo, stock.IdProducto, stock.Cantidad, sqlEx!.Message);
                return new ApiResponse<object> { IsSuccess = false, Message = sqlEx.Message };
            }

            _logger.LogInformation("Movimiento {Tipo} registrado: producto {IdProducto}, cantidad {Cantidad}",
                stock.Tipo, stock.IdProducto, stock.Cantidad);
            return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_REGISTER };
        }
    }
}
