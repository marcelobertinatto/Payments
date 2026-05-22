using Payment.Services.Notification.Infrastructure.Publisher.Interface;
using RabbitMQ.Client;
using System.Text;

namespace Payment.Services.Notification.Infrastructure.Publisher
{
    public class RabbitPublisher : IRabbitPublisher
    {
        private readonly IChannel _channel;

        public RabbitPublisher(IChannel channel)
        {
            _channel = channel;
        }

        public async Task PublishAsync(string queue,string message)
        {
            await _channel.QueueDeclareAsync(queue: queue,durable: true,exclusive: false,autoDelete: false);

            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(exchange: "", routingKey: queue, body: body);
        }
    }
}
