using Domain.Model;
using Domain.Model.Dto;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Services.Interface;
using Microsoft.Data.SqlClient;
using Utilities.Shared;

namespace Infrastructure.Services
{
    public class ProductoMovimientoStockService : IProductoStockService
    {
        private readonly ProductoStockRepository _categoriaRepository;
        private readonly IValidator<ProductoMovimientoStock> _validator;
        private readonly IMasterData _masterDataValidator;
        public ProductoMovimientoStockService(ProductoStockRepository categoriaRepository, IValidator<ProductoMovimientoStock> validator, IMasterData masterDataValidator)
        {
            _categoriaRepository = categoriaRepository;
            _validator = validator;
            _masterDataValidator = masterDataValidator;
        }

        //Para pruebas unitarias, descomenta este constructor y comenta el constructor anterior.

        /*readonly ICategoriaRepository _categoriaRepository;
        private readonly IValidator<Categorium> _validator;

        public CategoriaService(ICategoriaRepository categoriaRepository, IValidator<Categorium> validator)
        {
            _categoriaRepository = categoriaRepository;
            _validator = validator;
        }*/

        public async Task<ApiResponse<object>> RegistrarMovimientoAsync(ProductoMovimientoStock stock)
        {
            if (stock == null)
                return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_NULL };

            var validationResult = await _validator.ValidateAsync(stock);
            if (!validationResult.IsValid)
                return new ApiResponse<object> { IsSuccess = false, Message = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage)) };

            if (!await _masterDataValidator.ProductoExisteAsync(stock.IdProducto))
                return new ApiResponse<object> { IsSuccess = false, Message = "El producto no existe" };

            var result = await _categoriaRepository.RegistrarMovimientoAsync(stock);
            if (result > -1)
                return new ApiResponse<object> { IsSuccess = true, Message = Mensajes.MESSAGE_UPDATE };

            return new ApiResponse<object> { IsSuccess = false, Message = Mensajes.MESSAGE_UPDATE_FAILLED };
        }
    }
}
