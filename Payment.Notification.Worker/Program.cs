using Payment.Services.Notification.Infrastructure.DependencyInjection;
using Payment.Services.Notification.Infrastructure.Initializer;
using Payment.Services.Notification.Worker.Workers;
using Payment.Services.NotificationServices;
using Payment.Services.NotificationServices.Interface;

namespace Payment.Services.Notification.Worker
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddRabbitMq();

            builder.Services.AddHostedService<KafkaToRabbitMQWorker>();
            builder.Services.AddHostedService<RabbitMQNotificationProcessorWorker>();
            builder.Services.AddSingleton<INotificationService,NotificationService>();


            var host = builder.Build();

            using (var scope = host.Services.CreateScope())
            {
                var rabbitInitializer = scope.ServiceProvider.GetRequiredService<RabbitMqInitializer>();

                await rabbitInitializer.InitializeAsync();
            }

            await host.RunAsync();
        }
    }
}
