using BuildingBlocks.Contracts;
using Confluent.Kafka;
using Payment.Services.Domain.Interfaces;
using System.Text.Json;

namespace Payment.Services.Worker.Workers
{
    public class KafkaConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventBus _eventBus;

        public KafkaConsumerWorker(IServiceScopeFactory scopeFactory, IEventBus eventBus)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",
                GroupId = "payment-worker",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe("payments-created");

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                var evt = JsonSerializer.Deserialize<PaymentCreatedEvent>(result.Message.Value);

                using var scope = _scopeFactory.CreateScope();

                var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                var payment = await repo.GetAsync(evt.id);

                payment.Complete();

                await repo.SaveAsync(payment);

                await _eventBus.PublishAsync(
                "payments-completed",
                payment.Id,
                new PaymentCompletedEvent(
                    payment.Id,
                    evt.correlationId));
            }
        }
    }
}
