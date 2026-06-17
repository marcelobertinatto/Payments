using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using BuildingBlocks.Idempotency.Models;
using BuildingBlocks.Idempotency.Repository.Interfaces;
using BuildingBlocks.Middleware.Exceptions;

namespace BuildingBlocks.Idempotency.Repository
{
    public class DbIdempotencyRepository : IIdempotencyRepository
    {
        private readonly IDynamoDBContext _context;

        public DbIdempotencyRepository(IDynamoDBContext context)
        {
            _context = context;
        }
        public async Task<IdempotencyRecord?> GetAsync(string idempotencyKey, CancellationToken cancellationToken)
        {
            return await _context.LoadAsync<IdempotencyRecord>(idempotencyKey, cancellationToken);
        }

        public async Task<bool> TryAcquireAsync(string idempotencyKey, string type, string referenceId, TimeSpan ttl, CancellationToken cancellationToken)
        {
            var record = new IdempotencyRecord
            {
                IdempotencyKey = idempotencyKey,
                Type = type,
                ReferenceId = referenceId,
                CreatedAtUtc = DateTime.UtcNow.ToString("O"),
                ExpiresAt = DateTimeOffset.UtcNow.Add(ttl).ToUnixTimeSeconds()
            };

            Document document = _context.ToDocument(record);

            var config = new PutItemOperationConfig
            {
                ConditionalExpression = new Expression
                {
                    ExpressionStatement = "attribute_not_exists(IdempotencyKey)"
                }
            };

            try
            {
                var table = _context.GetTargetTable<IdempotencyRecord>();

                await table.PutItemAsync(
                    document,
                    config,
                    cancellationToken);

                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                return false;
            }
        }

        public async Task UpdateReferenceIdAsync(string idempotencyKey, string referenceId, CancellationToken cancellationToken)
        {
            var record = await GetAsync(idempotencyKey,cancellationToken);

            if (record is null)
            {
                throw new AppException($"Idempotency key '{idempotencyKey}' not found.");
            }

            record.ReferenceId = referenceId;

            await _context.SaveAsync(record,cancellationToken);
        }
    }
}
