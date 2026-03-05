using Domain.Model;
using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesServices;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Shared;

namespace APIRestSistemaVentas.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientoStockController : ControllerBase
    {
        private readonly ProductoMovimientoStockService _movimientoStockService;

        public MovimientoStockController(ProductoMovimientoStockService categoriaService)
        {
            _movimientoStockService = categoriaService;
        }

        //Para pruebas unitarias, descomenta este constructor y comenta el constructor anterior.

        /*private readonly ICategoriaService _movimientoStockService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _movimientoStockService = categoriaService;
        }*/

        [HttpPost]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] ProductoMovimientoStock stock)
        {
            var response = await _movimientoStockService.RegistrarMovimientoAsync(stock);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
