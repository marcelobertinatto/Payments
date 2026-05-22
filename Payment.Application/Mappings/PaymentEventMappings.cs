using BuildingBlocks.Contracts;

namespace Payment.Services.Application.Mappings
{
    public static class PaymentEventMappings
    {
        public static PaymentCreatedEvent ToCreatedEvent(
        this Domain.Model.Payment payment,
        string correlationId)
        {
            return new PaymentCreatedEvent(
                payment.Id,
                correlationId,
                payment.Amount,
                payment.Currency,
                payment.CustomerEmail,
                payment.Status
            );
        }
    }
}
