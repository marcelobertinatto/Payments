namespace Payment.Services.Notification.Infrastructure.Setup
{
    public static class RabbitMqQueues
    {
        public const string Notification = "notification-queue";

        public const string NotificationDlq = "notification-dlq";
    }
}
