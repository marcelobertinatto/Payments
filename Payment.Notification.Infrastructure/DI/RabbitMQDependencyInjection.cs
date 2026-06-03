using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Services.Notification.Infrastructure.Initializer;
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
                var configuration = _.GetRequiredService<IConfiguration>();
                var hostName = configuration["RabbitMQ:HostName"]!;
                return new ConnectionFactory
                {
                    HostName = hostName
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

                    IChannel channel = connection
                        .CreateChannelAsync()
                        .GetAwaiter()
                        .GetResult();

                    return channel;
                });

            services.AddSingleton<IRabbitPublisher, RabbitPublisher>();
            services.AddSingleton<RabbitMqInitializer>();

            return services;
        }
    }
}
