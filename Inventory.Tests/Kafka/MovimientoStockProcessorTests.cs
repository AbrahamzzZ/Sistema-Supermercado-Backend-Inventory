using Domain.Model.Dto;
using Infrastructure.Kafka;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Inventory.Tests.Kafka
{
    [TestClass]
    public class MovimientoStockProcessorTests
    {
        private Mock<IProductoStockRepository> _repositorio = null!;
        private MovimientoStockProcessor _processor = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _repositorio = new Mock<IProductoStockRepository>();
            _processor = new MovimientoStockProcessor(new ProductoStockValidator(), _repositorio.Object, NullLogger<MovimientoStockProcessor>.Instance);
        }

        [TestMethod]
        public async Task Procesar_MensajeDeVenta_RegistraLaSalidaConElUsuarioDelMensaje()
        {
            const string json = """{"IdProducto":5,"Tipo":"SALIDA","Cantidad":3,"PrecioCompra":null,"PrecioVenta":2.5,"Referencia":"VENTA","IdUsuario":7}""";

            var resultado = await _processor.ProcesarAsync(json);

            Assert.AreEqual(ResultadoMovimiento.Registrado, resultado);
            _repositorio.Verify(r => r.RegistrarMovimientoAsync(
                It.Is<ProductoMovimientoStock>(m => m.IdProducto == 5 && m.Tipo == "SALIDA" && m.Cantidad == 3 && m.PrecioVenta == 2.5m),
                7), Times.Once);
        }

        [TestMethod]
        public async Task Procesar_JsonInvalido_SeDescartaSinTocarLaBase()
        {
            var resultado = await _processor.ProcesarAsync("esto no es json");

            Assert.AreEqual(ResultadoMovimiento.Descartado, resultado);
            _repositorio.Verify(r => r.RegistrarMovimientoAsync(It.IsAny<ProductoMovimientoStock>(), It.IsAny<int?>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow("""{"idProducto":5,"tipo":"DEVOLUCION","cantidad":1,"referencia":"VENTA"}""", DisplayName = "Tipo desconocido")]
        [DataRow("""{"idProducto":5,"tipo":"ENTRADA","cantidad":0,"referencia":"COMPRA"}""", DisplayName = "Cantidad cero")]
        [DataRow("""{"idProducto":0,"tipo":"ENTRADA","cantidad":2,"referencia":"COMPRA"}""", DisplayName = "Producto inválido")]
        public async Task Procesar_MovimientoInvalido_SeDescartaSinTocarLaBase(string json)
        {
            var resultado = await _processor.ProcesarAsync(json);

            Assert.AreEqual(ResultadoMovimiento.Descartado, resultado);
            _repositorio.Verify(r => r.RegistrarMovimientoAsync(It.IsAny<ProductoMovimientoStock>(), It.IsAny<int?>()), Times.Never);
        }

        [TestMethod]
        public async Task Procesar_ErrorTecnicoDeLaBase_SePropagaParaQueElConsumidorReintente()
        {
            _repositorio.Setup(r => r.RegistrarMovimientoAsync(It.IsAny<ProductoMovimientoStock>(), It.IsAny<int?>()))
                        .ThrowsAsync(new TimeoutException("SQL Server no responde"));
            const string json = """{"idProducto":5,"tipo":"ENTRADA","cantidad":2,"referencia":"COMPRA"}""";

            await Assert.ThrowsExceptionAsync<TimeoutException>(() => _processor.ProcesarAsync(json));
        }
    }
}
