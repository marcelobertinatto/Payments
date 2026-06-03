using Microsoft.Extensions.Logging;
using Payment.Services.Notification.Infrastructure.Publisher.Interface;
using RabbitMQ.Client;
using System.Text;

namespace Payment.Services.Notification.Infrastructure.Publisher
{
    public class RabbitPublisher : IRabbitPublisher
    {
        private readonly IChannel _channel;
        private readonly ILogger<RabbitPublisher> _logger;

        public RabbitPublisher(IChannel channel, ILogger<RabbitPublisher> logger)
        {
            _channel = channel;
            _logger = logger;
        }

        public async Task PublishAsync(string queue,string message)
        {
            try
            {
                await _channel.QueueDeclareAsync(queue: queue, durable: true, exclusive: false, autoDelete: false);

                var body = Encoding.UTF8.GetBytes(message);

                if (body is null)
                {
                    _logger.LogError("Message RabbitMQ body is null.");
                }

                await _channel.BasicPublishAsync(exchange: "", routingKey: queue, body: body);
            }
            catch (Exception ex)
            {

                _logger.LogError($"An error occurred while publishing message to RabbitMQ. Error: {ex.Message}");
            }
        }
    }
}
