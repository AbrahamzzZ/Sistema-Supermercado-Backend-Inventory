using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System.Text;

namespace Infrastructure.Kafka
{
    public class MovimientoStockConsumer : BackgroundService
    {
        public const string Topic = "movimientos-stock";
        public const string CorrelationHeader = "X-Correlation-Id";

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
                EnableAutoCommit = false
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            Task.Factory.StartNew(() => ConsumirAsync(stoppingToken), stoppingToken, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

        private async Task ConsumirAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config)
                .SetErrorHandler((_, e) => _logger.LogWarning("Kafka: {Reason}", e.Reason))
                .Build();

            consumer.Subscribe(Topic);
            _logger.LogInformation("Consumidor Kafka suscrito a {Topic}", Topic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string> result;
                    try
                    {
                        result = consumer.Consume(stoppingToken);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogWarning(ex, "Error consumiendo de Kafka, reintentando en 5s");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }

                    var correlationId = LeerHeader(result.Message.Headers, CorrelationHeader) ?? Guid.NewGuid().ToString("N");

                    using (LogContext.PushProperty("CorrelationId", correlationId))
                    using (LogContext.PushProperty("RequestPath", $"kafka/{result.Topic}"))
                    using (LogContext.PushProperty("RequestMethod", "CONSUME"))
                    {
                        try
                        {
                            await ProcesarAsync(result.Message.Value);
                            consumer.Commit(result);
                        }
                        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error técnico procesando el mensaje {Offset}: {Value}. Se reintentará en 5s.",
                                result.TopicPartitionOffset, result.Message.Value);
                            consumer.Seek(result.TopicPartitionOffset);
                            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        }
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
            using var scope = _scopeFactory.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<MovimientoStockProcessor>();
            await processor.ProcesarAsync(json);
        }

        private static string? LeerHeader(Headers? headers, string nombre)
        {
            if (headers is not null && headers.TryGetLastBytes(nombre, out var bytes) && bytes is { Length: > 0 })
                return Encoding.UTF8.GetString(bytes);
            return null;
        }
    }
}
