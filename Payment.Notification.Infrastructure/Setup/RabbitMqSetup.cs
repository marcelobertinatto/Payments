using RabbitMQ.Client;

namespace Payment.Services.Notification.Infrastructure.Setup
{
    public static class RabbitMqSetup
    {
        public static async Task ConfigureAsync(IChannel channel)
        {
            await channel.QueueDeclareAsync(
                queue: RabbitMqQueues.Notification,
                durable: true,
                exclusive: false,
                autoDelete: false);

            await channel.QueueDeclareAsync(
                queue: RabbitMqQueues.NotificationDlq,
                durable: true,
                exclusive: false,
                autoDelete: false);
        }
    }
}
