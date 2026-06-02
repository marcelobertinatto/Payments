namespace BuildingBlocks.Messaging.Persistence.Repository.Interface
{
    public interface IProcessedEventRepository
    {
        Task<bool> ExistsAsync(Guid eventId);

        Task SaveAsync(Guid eventId, string processedAtUtc);
        
        Task<bool> TryAcquireAsync(Guid eventId,CancellationToken cancellationToken);
        
    }
}
