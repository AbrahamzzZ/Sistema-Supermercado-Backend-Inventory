using Domain.Model.Dto;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Services.Interface;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Utilities.Shared;

namespace Inventory.Tests.Services
{
    [TestClass]
    public class ProductoMovimientoStockServiceTests
    {
        private Mock<IProductoStockRepository> _repositorio = null!;
        private Mock<IMasterData> _maestros = null!;
        private ProductoMovimientoStockService _servicio = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _repositorio = new Mock<IProductoStockRepository>();
            _maestros = new Mock<IMasterData>();
            _maestros.Setup(m => m.ProductoExisteAsync(It.IsAny<int>())).ReturnsAsync(true);

            var usuarioActual = new Mock<ICurrentUserService>();
            usuarioActual.Setup(u => u.IdUsuario).Returns(2);

            _servicio = new ProductoMovimientoStockService(_repositorio.Object, new ProductoStockValidator(), _maestros.Object,
                usuarioActual.Object, NullLogger<ProductoMovimientoStockService>.Instance);
        }

        private static ProductoMovimientoStock Movimiento() => new()
        {
            IdProducto = 5, Tipo = "ENTRADA", Cantidad = 10, Referencia = "AJUSTE", PrecioCompra = 1, PrecioVenta = 2
        };

        [TestMethod]
        public async Task RegistrarMovimiento_Correcto_RegistraConElUsuarioDelToken()
        {
            var resultado = await _servicio.RegistrarMovimientoAsync(Movimiento());

            Assert.IsTrue(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_REGISTER, resultado.Message);
            _repositorio.Verify(r => r.RegistrarMovimientoAsync(It.IsAny<ProductoMovimientoStock>(), 2), Times.Once);
        }

        [TestMethod]
        public async Task RegistrarMovimiento_ProductoInexistenteEnAdmin_NoRegistra()
        {
            _maestros.Setup(m => m.ProductoExisteAsync(5)).ReturnsAsync(false);

            var resultado = await _servicio.RegistrarMovimientoAsync(Movimiento());

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual("El producto no existe", resultado.Message);
            _repositorio.Verify(r => r.RegistrarMovimientoAsync(It.IsAny<ProductoMovimientoStock>(), It.IsAny<int?>()), Times.Never);
        }

        [TestMethod]
        public async Task RegistrarMovimiento_TipoInvalido_NoConsultaAdminNiRegistra()
        {
            var movimiento = Movimiento();
            movimiento.Tipo = "OTRO";

            var resultado = await _servicio.RegistrarMovimientoAsync(movimiento);

            Assert.IsFalse(resultado.IsSuccess);
            _maestros.Verify(m => m.ProductoExisteAsync(It.IsAny<int>()), Times.Never);
            _repositorio.Verify(r => r.RegistrarMovimientoAsync(It.IsAny<ProductoMovimientoStock>(), It.IsAny<int?>()), Times.Never);
        }
    }
}
