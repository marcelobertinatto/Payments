using BuildingBlocks.Contracts;
using Confluent.Kafka;
using Payment.Services.Notification.Infrastructure.Publisher;
using Payment.Services.Notification.Infrastructure.Publisher.Interface;
using System.Text.Json;

namespace Payment.Services.Notification.Worker.Workers
{
    public class KafkaToRabbitMQWorker : BackgroundService
    {
        private readonly IRabbitPublisher _rabbitPublisher;
        public KafkaToRabbitMQWorker(IRabbitPublisher rabbitPublisher)
        {
            _rabbitPublisher = rabbitPublisher;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",
                GroupId = "notification-worker",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe("payments-completed");

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(result.Message.Value);

                var message = JsonSerializer.Serialize(evt);

                await _rabbitPublisher.PublishAsync("notifications",message);
            }
        }
    }
}
