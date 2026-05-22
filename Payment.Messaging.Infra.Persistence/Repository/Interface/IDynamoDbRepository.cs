using BuildingBlocks.Messaging.Persistence.Model;

namespace BuildingBlocks.Messaging.Persistence.Repository.Interface
{
    public interface IDynamoDbRepository
    {
        Task SaveAsync(PaymentEntity entity);
        Task<PaymentEntity> GetAsync(string id);
    }
}
