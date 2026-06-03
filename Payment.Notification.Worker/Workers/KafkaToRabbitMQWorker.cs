using BuildingBlocks.Contracts;
using BuildingBlocks.Middleware.Exceptions;
using Confluent.Kafka;
using Payment.Services.Notification.Infrastructure.Publisher.Interface;
using Payment.Services.Notification.Infrastructure.Setup;
using System.Text.Json;

namespace Payment.Services.Notification.Worker.Workers
{
    public class KafkaToRabbitMQWorker : BackgroundService
    {
        private readonly IRabbitPublisher _rabbitPublisher;
        private readonly ILogger<KafkaToRabbitMQWorker> _logger;
        public KafkaToRabbitMQWorker(IRabbitPublisher rabbitPublisher, ILogger<KafkaToRabbitMQWorker> logger)
        {
            _rabbitPublisher = rabbitPublisher;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",
                GroupId = "notification-worker",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe("payments-completed");

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(result.Message.Value);

                if (evt is null) 
                { 
                    _logger.LogError("Message in Kafka is null.");
                    return;
                }

                var message = JsonSerializer.Serialize(evt);

                await _rabbitPublisher.PublishAsync(RabbitMqQueues.Notification, message);

                consumer.Commit();

                _logger.LogInformation("Kafka to RabbitMQ read committed.");
            }
        }
    }
}
