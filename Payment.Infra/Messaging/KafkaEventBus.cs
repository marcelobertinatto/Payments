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
                BootstrapServers = "localhost:29092"
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }
        public async Task PublishAsync(string topic, string key, object message)
        {
            var json = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(topic,
            new Message<string, string>
            {
                Key = key,
                Value = json
            });
        }
    }
}
