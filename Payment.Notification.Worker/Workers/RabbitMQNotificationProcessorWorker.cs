using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Payment.Services.Notification.Worker.Workers
{
    public class RabbitMQNotificationProcessorWorker : BackgroundService
    {
        private readonly IChannel _channel;

        public RabbitMQNotificationProcessorWorker(IChannel channel)
        {
            _channel = channel;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _channel.QueueDeclareAsync(queue: "notifications",durable: true,exclusive: false,autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model,ea) =>
            {
                var body = ea.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"Sending notification: {message}");

                // TODO:
                // AWS SES
                // AWS SNS
                // Twilio
                // SMTP

                await Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(queue: "notifications",autoAck: true,consumer: consumer);
        }
    }
}
