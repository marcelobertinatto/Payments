using BuildingBlocks.Contracts;
using BuildingBlocks.Idempotency.Constants;
using BuildingBlocks.Idempotency.Services;
using BuildingBlocks.Idempotency.Services.Interfaces;
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
            PaymentCreatedEvent? evt = null;

            using var scope = _scopeFactory.CreateScope();

            //this is used for injecting the dependencies in the handlers as scoped
            var idempotencyService = scope.ServiceProvider.GetRequiredService<IIdempotencyService>();

            try
            {
                evt = JsonSerializer.Deserialize<PaymentCreatedEvent>(result.Message.Value);

                if (evt is null)
                {
                    _logger.LogWarning($"Message in Kafka is null.");
                    return;
                }

                var record = await idempotencyService.GetAsync(evt.EventId.ToString(),cancellationToken);

                //if the record is null means that 
                if (record is null)
                {
                    //process the event and mark it as processing into idempotency table
                    await idempotencyService
                        .TryAcquireAsync(evt.EventId.ToString(),
                        IdempotencyTypes.PaymentCreatedEvent, evt.EventId.ToString(),
                        IdempotencyTtl.EventProcessing, cancellationToken);
                }
                else if (record.Status == IdempotencyStatus.Completed)
                {
                    //if it's completed, skip it
                    _logger.LogInformation("Event already processed. EventId={EventId}",evt.EventId);

                    return;
                }
                else if (record.Status == IdempotencyStatus.Failed)
                {
                    _logger.LogWarning("Retrying failed event. EventId={EventId}",evt.EventId);
                }
                else if (record.Status == IdempotencyStatus.Processing)
                {
                    var updatedAt = DateTime.Parse(record.UpdatedAtUtc);

                    if (updatedAt > DateTime.UtcNow.AddMinutes(-5))
                    {
                        _logger.LogInformation("Event still being processed. EventId={EventId}",
                            evt.EventId);

                        return;
                    }

                    _logger.LogWarning("Stale processing record found. EventId={EventId}",
                        evt.EventId);
                }

                //inject PaymentRespository as scoped to get specific Payment.Id
                var paymentRepo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                //get specific payment.Id from Payment table in DynamoDB
                var payment = await paymentRepo.GetAsync(evt.id);

                //if payment does not exist, skip it
                if (payment is null)
                {
                    _logger.LogWarning($"Payment not found for ID: {evt.id}");

                    return;
                }

                //mark payment as completed in Payment table
                payment.Complete();

                //save it to Payment table in DynamoDB
                await paymentRepo.SaveAsync(payment);

                //mark the event as completed in OutboxEvents table to prevent duplicate processing
                await idempotencyService.MarkCompletedAsync(evt.EventId.ToString(), cancellationToken);

                //publish this completed payment into payments-completed topic in Kafka
                await _eventBus.PublishAsync(
                    "payments-completed",
                    payment.Id,
                    payment.ToCompletedEvent(evt.correlationId));

                
            }
            catch (Exception ex)
            {
                if (evt is not null)
                {
                    await idempotencyService.MarkFailedAsync(
                        evt.EventId.ToString(),
                        cancellationToken);
                    _logger.LogError(ex, "Failed processing event {EventId}", evt.EventId);

                    throw;
                }
                _logger.LogError($"There is an error happening. Error: '{ex.Message}'.");
            }
        }
    }
}
