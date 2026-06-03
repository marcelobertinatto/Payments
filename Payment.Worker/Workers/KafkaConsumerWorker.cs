using BuildingBlocks.Contracts;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;
using BuildingBlocks.Middleware.Exceptions;
using Confluent.Kafka;
using Payment.Services.Application.Mappings;
using Payment.Services.Domain.Interfaces;
using System.Text.Json;

namespace Payment.Services.Worker.Workers
{
    public class KafkaConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEventBus _eventBus;
        private readonly ILogger<KafkaConsumerWorker> _logger;

        public KafkaConsumerWorker(IServiceScopeFactory scopeFactory, IEventBus eventBus, ILogger<KafkaConsumerWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _eventBus = eventBus;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",
                GroupId = "payment-worker",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe("payments-created");

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                try
                {
                    await ProcessMessageAsync(result,stoppingToken);

                    consumer.Commit(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"There is an error happening. Error: '{ex.Message}'.");
                }
            }
        }

        private async Task ProcessMessageAsync(ConsumeResult<string, string> result,CancellationToken cancellationToken)
        {
            try
            {
                var evt = JsonSerializer.Deserialize<PaymentCreatedEvent>(result.Message.Value);

                if (evt is null)
                {
                    _logger.LogWarning($"Message in Kafka is null.");
                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var processedRepo = scope.ServiceProvider.GetRequiredService<IProcessedEventRepository>();

                var acquired = await processedRepo.TryAcquireAsync(evt.EventId, cancellationToken);

                if (!acquired)
                {
                    _logger.LogWarning("Duplicate event {EventId}", evt.EventId);

                    return;
                }

                var paymentRepo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                var payment = await paymentRepo.GetAsync(evt.id);

                if (payment is null)
                {
                    _logger.LogWarning($"Payment not found for ID: {evt.id}");

                    return;
                }

                payment.Complete();

                await paymentRepo.SaveAsync(payment);

                await _eventBus.PublishAsync(
                    "payments-completed",
                    payment.Id,
                    payment.ToCompletedEvent(evt.correlationId));
            }
            catch (Exception ex)
            {
                _logger.LogError($"There is an error happening. Error: '{ex.Message}'.");
            }
        }
    }
}
