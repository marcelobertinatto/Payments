using BuildingBlocks.Middleware.Exceptions;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Payment.Services.Domain.Interfaces;
using System.Text.Json;

namespace Payment.Services.Infra.Messaging
{
    public class KafkaEventBus : IEventBus
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaEventBus> _logger;

        public KafkaEventBus(ILogger<KafkaEventBus> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:29092",
                MessageTimeoutMs = 3000,
                RequestTimeoutMs = 3000,
                SocketTimeoutMs = 3000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }
        public async Task PublishAsync<T>(string topic, string key, T message)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                await _producer.ProduceAsync(topic,
                new Message<string, string>
                {
                    Key = key,
                    Value = json
                }, cts.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"There is an error happening. Error: '{ex.Message}'.");
            }
        }

        public async Task PublishRawAsync(string topic, string key, string json)
        {
            await _producer.ProduceAsync(topic,
            new Message<string, string>
            {
                Key = key,
                Value = json
            });
        }
    }
}
