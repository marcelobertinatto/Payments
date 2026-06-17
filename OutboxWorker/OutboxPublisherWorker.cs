using BuildingBlocks.Messaging.Persistence.Repository.Interface;
using Payment.Services.Domain.Interfaces;

namespace Payment.Services.OutboxWorker
{
    public class OutboxPublisherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherWorker> _logger;
        public OutboxPublisherWorker( IServiceScopeFactory scopeFactory, ILogger<OutboxPublisherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var outboxRepo = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

                var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                //just grab from outboxEvents table and publish into Kafka topic: payments-created
                var pendingEvents = await outboxRepo.GetPendingAsync();

                foreach (var evt in pendingEvents)
                {
                    try
                    {
                        //publish into kafka topic: payments-created
                        await eventBus.PublishRawAsync("payments-created",evt.EventId,evt.Payload);

                        //mark as processed
                        await outboxRepo.MarkProcessedAsync(evt.EventId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed publishing outbox event {EventId}", evt.EventId);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5),stoppingToken);
            }
        }
    }
}
