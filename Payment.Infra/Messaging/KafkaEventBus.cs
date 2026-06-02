using BuildingBlocks.Middleware.Exceptions;
using Confluent.Kafka;
using Payment.Services.Domain.Interfaces;
using System.Text.Json;

namespace Payment.Services.Infra.Messaging
{
    public class KafkaEventBus : IEventBus
    {
        private readonly IProducer<string, string> _producer;

        public KafkaEventBus()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:29092",
                MessageTimeoutMs = 3000,
                RequestTimeoutMs = 3000,
                SocketTimeoutMs = 3000
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }
        public async Task PublishAsync(string topic, string key, object message)
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
                throw new EventBusException($"Unable to publish event to topic '{topic}'. Error: '{ex.Message}'.");
            }
        }
    }
}
