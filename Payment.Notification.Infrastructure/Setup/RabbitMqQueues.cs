namespace Payment.Services.Notification.Infrastructure.Setup
{
    public static class RabbitMqQueues
    {
        public const string Notification = "notification-queue";
        public const string NotificationRetry = "notification-retry";
        public const string NotificationDlq = "notification-dlq";
    }
}
