using Payment.Services.Notification.Infrastructure.DependencyInjection;
using Payment.Services.Notification.Infrastructure.Publisher;
using Payment.Services.Notification.Infrastructure.Publisher.Interface;
using Payment.Services.Notification.Worker.Workers;

namespace Payment.Services.Notification.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddRabbitMq();

            builder.Services.AddSingleton<IRabbitPublisher, RabbitPublisher>();

            builder.Services.AddHostedService<KafkaToRabbitMQWorker>();
            builder.Services.AddHostedService<RabbitMQNotificationProcessorWorker>();
            

            var host = builder.Build();
            host.Run();
        }
    }
}
