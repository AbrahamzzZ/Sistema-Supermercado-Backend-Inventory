namespace Infrastructure.Services.Interface
{
    public interface IMasterData
    {
        Task<bool> ProductoExisteAsync(int idProducto);
    }
}
