using BuildingBlocks.Idempotency.Models;

namespace BuildingBlocks.Idempotency.Repository.Interfaces
{
    public interface IIdempotencyRepository
    {
        Task<bool> TryAcquireAsync(string idempotencyKey, string type, string referenceId, TimeSpan ttl, CancellationToken cancellationToken);

        Task<IdempotencyRecord?> GetAsync(string idempotencyKey, CancellationToken cancellationToken);

        Task UpdateReferenceIdAsync(string idempotencyKey,string referenceId,CancellationToken cancellationToken);
    }
}
