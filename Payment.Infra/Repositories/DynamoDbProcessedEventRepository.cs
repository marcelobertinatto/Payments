using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using BuildingBlocks.Messaging.Persistence.Model;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;

namespace Payment.Services.Infra.Repositories
{
    public class DynamoDbProcessedEventRepository : IProcessedEventRepository
    {
        private readonly IDynamoDBContext _context;

        public DynamoDbProcessedEventRepository(IDynamoDBContext context)
        {
            _context = context;
        }
        public async Task<bool> ExistsAsync(Guid eventId)
        {
            var result = await _context.LoadAsync<ProcessedEvent>(eventId.ToString());
            return result != null;
        }

        public async Task SaveAsync(Guid eventId, string processedAtUtc)
        {
            var processedEvent = new ProcessedEvent
            {
                EventId = eventId.ToString(),
                ProcessedAtUtc = processedAtUtc
            };

            await _context.SaveAsync(processedEvent);
        }

        public async Task<bool> TryAcquireAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var processedEvent = new ProcessedEvent
            {
                EventId = eventId.ToString(),
                ProcessedAtUtc = DateTime.UtcNow.ToString("O")
            };

            Document document = _context.ToDocument(processedEvent);

            var config = new PutItemOperationConfig
            {
                // Enforces that this EventId must not already exist
                ConditionalExpression = new Expression
                {
                    ExpressionStatement = "attribute_not_exists(EventId)",
                    ExpressionAttributeNames = new Dictionary<string, string>(),
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>()
                }
            };

            try
            {
                var table = _context.GetTargetTable<ProcessedEvent>();
                await table.PutItemAsync(document, config, cancellationToken);

                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                return false;
            }
        }
    }
}
