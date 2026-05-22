using Microsoft.Extensions.DependencyInjection;
using Payment.Services.Notification.Infrastructure.Publisher;
using Payment.Services.Notification.Infrastructure.Publisher.Interface;
using RabbitMQ.Client;

namespace Payment.Services.Notification.Infrastructure.DependencyInjection
{
    public static class RabbitMQDependencyInjection
    {
        public static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionFactory>(_ =>
            {
                return new ConnectionFactory
                {
                    HostName = "localhost"
                };
            });

            services.AddSingleton<IConnection>(provider =>
                {
                    var factory = provider.GetRequiredService<IConnectionFactory>();

                    return factory
                        .CreateConnectionAsync()
                        .GetAwaiter()
                        .GetResult();
                });

            services.AddSingleton<IChannel>(provider =>
                {
                    var connection = provider.GetRequiredService<IConnection>();

                    return connection
                        .CreateChannelAsync()
                        .GetAwaiter()
                        .GetResult();
                });

            services.AddSingleton<IRabbitPublisher, RabbitPublisher>();

            return services;
        }
    }
}
