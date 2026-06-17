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

            await channel.QueueDeclareAsync(
            queue: RabbitMqQueues.NotificationRetry,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = 30000,

                ["x-dead-letter-exchange"] = "",

                ["x-dead-letter-routing-key"] = RabbitMqQueues.Notification
            });
        }
    }
}
