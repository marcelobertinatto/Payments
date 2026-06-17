using BuildingBlocks.Idempotency.Models;
using BuildingBlocks.Idempotency.Repository.Interfaces;
using BuildingBlocks.Idempotency.Services.Interfaces;

namespace BuildingBlocks.Idempotency.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IIdempotencyRepository _repository;

        public IdempotencyService(IIdempotencyRepository repository)
        {
            _repository = repository;
        }

        public async Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken)
        {
            return await _repository.GetAsync(key, cancellationToken);
        }

        public async Task<bool> TryAcquireAsync(string key, string type, string referenceId, TimeSpan ttl, CancellationToken cancellationToken)
        {
            return await _repository.TryAcquireAsync(key, type, referenceId, ttl, cancellationToken);
        }

        public async Task UpdateReferenceIdAsync(string idempotencyKey, string referenceId, CancellationToken cancellationToken)
        {
            await _repository.UpdateReferenceIdAsync(idempotencyKey, referenceId, cancellationToken);
        }
    }
}
