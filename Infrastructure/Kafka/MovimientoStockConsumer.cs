using Confluent.Kafka;
using Domain.Model.Dto;
using FluentValidation;
using Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Kafka
{
    public class MovimientoStockConsumer : BackgroundService
    {
        public const string Topic = "movimientos-stock";

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MovimientoStockConsumer> _logger;
        private readonly ConsumerConfig _config;

        public MovimientoStockConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<MovimientoStockConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = configuration["Kafka:GroupId"] ?? "inventory-service",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                // Confirmamos el offset manualmente solo cuando el movimiento se guardó
                EnableAutoCommit = false
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            Task.Factory.StartNew(() => ConsumirAsync(stoppingToken), stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

        private async Task ConsumirAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config).SetErrorHandler((_, e) => _logger.LogWarning("Kafka: {Reason}", e.Reason)).Build();

            consumer.Subscribe(Topic);
            _logger.LogInformation("Consumidor Kafka suscrito a {Topic}", Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;
                    try
                    {
                        result = consumer.Consume(stoppingToken);
                        await ProcesarAsync(result.Message.Value);
                        consumer.Commit(result);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogWarning(ex, "Error consumiendo de Kafka, reintentando en 5s");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando el mensaje {Value}. Se reintentará.", result?.Message.Value);
                        if (result is not null)
                            consumer.Seek(result.TopicPartitionOffset);
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                consumer.Close();
            }
        }

        private async Task ProcesarAsync(string json)
        {
            ProductoMovimientoStock? movimiento;
            try
            {
                movimiento = JsonSerializer.Deserialize<ProductoMovimientoStock>(json, JsonOptions);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Mensaje inválido descartado: {Json}", json);
                return;
            }

            if (movimiento is null)
                return;

            using var scope = _scopeFactory.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<ProductoMovimientoStock>>();
            var repository = scope.ServiceProvider.GetRequiredService<ProductoStockRepository>();

            var validacion = await validator.ValidateAsync(movimiento);
            if (!validacion.IsValid)
            {
                _logger.LogWarning("Movimiento descartado por validación: {Errores}",
                    string.Join(" | ", validacion.Errors.Select(e => e.ErrorMessage)));
                return;
            }
            
            await repository.RegistrarMovimientoAsync(movimiento);

            _logger.LogInformation("Movimiento {Tipo} registrado: producto {IdProducto}, cantidad {Cantidad}",
                movimiento.Tipo, movimiento.IdProducto, movimiento.Cantidad);
        }
    }
}
