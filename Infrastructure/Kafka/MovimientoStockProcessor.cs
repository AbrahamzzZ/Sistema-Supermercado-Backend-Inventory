using Domain.Model.Dto;
using FluentValidation;
using Infrastructure.Repository;
using Infrastructure.Repository.InterfacesRepository;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Text.Json;

namespace Infrastructure.Kafka
{
    public enum ResultadoMovimiento
    {
        Registrado,
        Descartado
    }

    public class MovimientoStockProcessor
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IValidator<ProductoMovimientoStock> _validator;
        private readonly IProductoStockRepository _repository;
        private readonly ILogger<MovimientoStockProcessor> _logger;

        public MovimientoStockProcessor(IValidator<ProductoMovimientoStock> validator, IProductoStockRepository repository, ILogger<MovimientoStockProcessor> logger)
        {
            _validator = validator;
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResultadoMovimiento> ProcesarAsync(string json)
        {
            ProductoMovimientoStock? movimiento;
            try
            {
                movimiento = JsonSerializer.Deserialize<ProductoMovimientoStock>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Mensaje con JSON inválido descartado: {Json}", json);
                return ResultadoMovimiento.Descartado;
            }

            if (movimiento is null)
                return ResultadoMovimiento.Descartado;

            using (LogContext.PushProperty("IdUsuario", movimiento.IdUsuario))
            {
                var validacion = await _validator.ValidateAsync(movimiento);
                if (!validacion.IsValid)
                {
                    _logger.LogWarning("Movimiento descartado por validación: {Errores}",
                        string.Join(" | ", validacion.Errors.Select(e => e.ErrorMessage)));
                    return ResultadoMovimiento.Descartado;
                }

                try
                {
                    await _repository.RegistrarMovimientoAsync(movimiento, movimiento.IdUsuario);
                }
                catch (Exception ex) when (ErroresNegocioSql.EsErrorDeNegocio(ex, out var sqlEx))
                {
                    _logger.LogWarning("Movimiento {Tipo} descartado para el producto {IdProducto} (cantidad {Cantidad}): {Motivo}",
                        movimiento.Tipo, movimiento.IdProducto, movimiento.Cantidad, sqlEx!.Message);
                    return ResultadoMovimiento.Descartado;
                }

                _logger.LogInformation("Movimiento {Tipo} registrado: producto {IdProducto}, cantidad {Cantidad}",
                    movimiento.Tipo, movimiento.IdProducto, movimiento.Cantidad);
                return ResultadoMovimiento.Registrado;
            }
        }
    }
}
