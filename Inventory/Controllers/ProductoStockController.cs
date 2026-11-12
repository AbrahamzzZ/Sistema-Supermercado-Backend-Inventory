using Domain.Model.Dto;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilities.Shared;

namespace Inventory.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoStockController : ControllerBase
    {
        private readonly ProductoStockConsultaService _consultaService;

        public ProductoStockController(ProductoStockConsultaService consultaService)
        {
            _consultaService = consultaService;
        }

        // GET: api/ProductoStock (stock de todos los productos; lo usa Admin para el análisis con IA)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductoStockDto>>>> GetStocks()
            => Ok(await _consultaService.ListarStockAsync());

        // GET: api/ProductoStock/5
        [HttpGet("{idProducto:int}")]
        public async Task<ActionResult<ApiResponse<ProductoStockDto>>> GetStock(int idProducto)
        {
            var response = await _consultaService.ObtenerStockAsync(idProducto);
            return response.IsSuccess ? Ok(response) : NotFound(response);
        }

        // GET: api/ProductoStock/5/historial-precios
        [HttpGet("{idProducto:int}/historial-precios")]
        public async Task<ActionResult<ApiResponse<List<HistorialPrecioDto>>>> GetHistorialPrecios(int idProducto)
        {
            var response = await _consultaService.ListarHistorialPreciosAsync(idProducto);
            return Ok(response);
        }
    }
}
