using Infrastructure.Services.Interface;

namespace Infrastructure.Services
{
    public class MasterDataValidationService : IMasterData
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MasterDataValidationService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> ProductoExisteAsync(int idProducto)
        {
            var client = _httpClientFactory.CreateClient("AdminApi");
            var response = await client.GetAsync($"/admin/Producto/{idProducto}");
            return response.IsSuccessStatusCode;
        }
    }
}
