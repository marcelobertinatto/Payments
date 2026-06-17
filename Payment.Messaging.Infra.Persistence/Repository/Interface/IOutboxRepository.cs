using BuildingBlocks.Messaging.Persistence.Model;

namespace BuildingBlocks.Messaging.Persistence.Repository.Interface
{
    public interface IOutboxRepository
    {
        Task SaveAsync(OutboxEvent evt);

        Task<List<OutboxEvent>> GetPendingAsync();

        Task MarkProcessedAsync(string eventId);
    }
}
