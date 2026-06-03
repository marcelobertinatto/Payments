using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BuildingBlocks.Middleware.Exceptions;
using Microsoft.Extensions.Logging;
using Payment.Services.Domain.Interfaces;
using Payment.Services.Infra.Mappings;
using Payment.Services.Infra.Model;

namespace Payment.Services.Infra.Repositories
{
    public class DynamoDbPaymentRepository : IPaymentRepository
    {
        private readonly IDynamoDBContext _context;
        private readonly ILogger<DynamoDbPaymentRepository> _logger;

        public DynamoDbPaymentRepository(IDynamoDBContext context, ILogger<DynamoDbPaymentRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                _logger.LogError($"Unable to get data from DynamoDB. Error: '{ex.Message}'.");
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
                _logger.LogError($"Unable to save data to DynamoDB. Error: '{ex.Message}'.");
                throw new RepositoryException($"Unable to save data into DynamoDB. Error: '{ex.Message}'.");
            }
        }
    }
}
