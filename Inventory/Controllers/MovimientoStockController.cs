using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientoStockController : ControllerBase
    {
        private readonly IProductoStockService _movimientoStockService;

        public MovimientoStockController(IProductoStockService movimientoStockService)
        {
            _movimientoStockService = movimientoStockService;
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] ProductoMovimientoStock stock)
        {
            var response = await _movimientoStockService.RegistrarMovimientoAsync(stock);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
