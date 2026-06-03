using Payment.Services.Notification.Infrastructure.Setup;
using RabbitMQ.Client;

namespace Payment.Services.Notification.Infrastructure.Initializer
{
    public class RabbitMqInitializer
    {
        private readonly IChannel _channel;

        public RabbitMqInitializer(IChannel channel)
        {
            _channel = channel;
        }

        public async Task InitializeAsync()
        {
            await RabbitMqSetup.ConfigureAsync(_channel);
        }
    }
}
