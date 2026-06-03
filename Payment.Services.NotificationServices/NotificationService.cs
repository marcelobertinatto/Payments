using BuildingBlocks.Contracts;
using Microsoft.Extensions.Logging;
using Payment.Services.NotificationServices.Interface;

namespace Payment.Services.NotificationServices
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }
        public async Task SendPaymentCompletedAsync(PaymentCompletedEvent evt, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending notification for PaymentId {PaymentId}",evt.PaymentId);

            //throw new TimeoutException("SMTP timeout");

            //await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            await Task.Delay(1000, cancellationToken);

            _logger.LogInformation("Notification sent.");
        }
    }
}
