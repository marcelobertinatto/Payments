using BuildingBlocks.Idempotency.Models;

namespace BuildingBlocks.Idempotency.Services.Interfaces
{
    public interface IIdempotencyService
    {
        Task<bool> TryAcquireAsync(
        string key,
        string type,
        string referenceId,
        TimeSpan ttl,
        CancellationToken cancellationToken);

        Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken);

        Task UpdateReferenceIdAsync(string idempotencyKey, string referenceId, CancellationToken cancellationToken);
    }
}
