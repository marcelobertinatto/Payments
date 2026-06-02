using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BuildingBlocks.Middleware.Exceptions;
using Payment.Services.Domain.Interfaces;
using Payment.Services.Infra.Mappings;
using Payment.Services.Infra.Model;

namespace Payment.Services.Infra.Repositories
{
    public class DynamoDbPaymentRepository : IPaymentRepository
    {
        private readonly IDynamoDBContext _context;

        public DynamoDbPaymentRepository(IDynamoDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Services.Domain.Model.Payment> GetAsync(string paymentId)
        {
            try
            {
                var entity = await _context.LoadAsync<PaymentEntity>(paymentId);

                return entity.ToDomain();
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unable to get data from DynamoDB. Error: '{ex.Message}'.");
            }
        }

        public async Task SaveAsync(Services.Domain.Model.Payment payment)
        {
            try
            {
                var entity = payment.ToEntity();

                await _context.SaveAsync(entity);
            }
            catch (Exception ex)
            {
                throw new RepositoryException($"Unable to save data into DynamoDB. Error: '{ex.Message}'.");
            }
        }
    }
}
