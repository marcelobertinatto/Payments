using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using BuildingBlocks.Messaging.Persistence.Model;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;

namespace BuildingBlocks.Messaging.Persistence.Repository
{
    public class DynamoDbOutboxRepository : IOutboxRepository
    {
        private readonly IDynamoDBContext _context;

        public DynamoDbOutboxRepository(IDynamoDBContext context)
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000"
            };

            var client = new AmazonDynamoDBClient(config);
            _context = new DynamoDBContext(client);
        }

        public async Task<List<OutboxEvent>> GetPendingAsync()
        {
            var scan = _context.ScanAsync<OutboxEvent>(
                new[]
                {
                    new ScanCondition(nameof(OutboxEvent.Status),ScanOperator.Equal,"Pending")
                });

            return await scan.GetRemainingAsync();
        }

        public async Task MarkProcessedAsync(string eventId)
        {
            var evt = await _context.LoadAsync<OutboxEvent>(eventId);

            evt.Status = "Processed";

            await _context.SaveAsync(evt);
        }

        public Task SaveAsync(OutboxEvent evt)
        {
            return _context.SaveAsync(evt);
        }
    }
}
