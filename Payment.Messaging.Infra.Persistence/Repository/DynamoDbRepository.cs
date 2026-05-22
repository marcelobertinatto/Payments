using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BuildingBlocks.Messaging.Persistence.Model;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;

namespace BuildingBlocks.Messaging.Persistence.Repository
{
    public class DynamoDbRepository : IDynamoDbRepository
    {
        private readonly DynamoDBContext _context;
        public DynamoDbRepository()
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000"
            };

            var client = new AmazonDynamoDBClient(config);
            _context = new DynamoDBContext(client);
        }
        public async Task<PaymentEntity> GetAsync(string id)
        {
            return await _context.LoadAsync<PaymentEntity>(id);
        }

        public async Task SaveAsync(PaymentEntity entity)
        {
            await _context.SaveAsync(entity);
        }
    }
}
