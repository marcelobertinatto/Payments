using Confluent.Kafka;
using BuildingBlocks.Messaging.Kafka.Producer.Interface;
using System.Text.Json;

namespace BuildingBlocks.Messaging.Kafka.Producer
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IProducer<string,string> _producer;
        public KafkaProducer()
        {
            var config = new ProducerConfig
            {
                BootstrapServers = "localhost:29092",
                Acks = Acks.All,
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, string key, T message)
        {
            var json = JsonSerializer.Serialize(message);

            await _producer.ProduceAsync(topic, new Message<string, string> 
            { 
                Key = key, 
                Value = json 
            });
        }
    }
}
