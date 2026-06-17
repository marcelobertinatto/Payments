using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BuildingBlocks.Messaging.Persistence.Model;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;

namespace BuildingBlocks.Messaging.Persistence.Repository
{
    public class DynamoDbRepository : IDynamoDbRepository
    {
        private readonly IDynamoDBContext _context;
        public DynamoDbRepository(IDynamoDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
