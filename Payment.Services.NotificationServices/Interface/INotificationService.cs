using BuildingBlocks.Contracts;

namespace Payment.Services.NotificationServices.Interface
{
    public interface INotificationService
    {
        Task SendPaymentCompletedAsync(PaymentCompletedEvent evt,CancellationToken cancellationToken);
    }
}
