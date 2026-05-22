using Payment.Services.Infra.Model;

namespace Payment.Services.Infra.Mappings
{
    public static class PaymentEntityMappings
    {
        public static PaymentEntity ToEntity(this Domain.Model.Payment payment)
        {
            return new PaymentEntity
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CustomerEmail = payment.CustomerEmail,
                CorrelationId = payment.CorrelationId,
                Status = payment.Status
            };
        }

        public static Domain.Model.Payment ToDomain(this PaymentEntity entity)
        {
            return Domain.Model.Payment.Restore(
                entity.Id,
                entity.Amount,
                entity.Currency,
                entity.CustomerEmail,
                entity.Status,
                entity.CorrelationId);
        }
    }
}
