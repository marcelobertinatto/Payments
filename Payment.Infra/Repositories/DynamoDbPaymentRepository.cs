using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Payment.Services.Domain.Interfaces;
using Payment.Services.Infra.Mappings;
using Payment.Services.Infra.Model;

namespace Payment.Services.Infra.Repositories
{
    public class DynamoDbPaymentRepository : IPaymentRepository
    {
        private readonly DynamoDBContext _context;

        public DynamoDbPaymentRepository(IAmazonDynamoDB client)
        {
            _context = new DynamoDBContext(client);
        }

        public async Task<Services.Domain.Model.Payment> GetAsync(string paymentId)
        {
            var entity = await _context.LoadAsync<PaymentEntity>(paymentId);

            return entity.ToDomain();
        }

        public async Task SaveAsync(Services.Domain.Model.Payment payment)
        {
            var entity = payment.ToEntity();

            await _context.SaveAsync(entity);
        }
    }
}
