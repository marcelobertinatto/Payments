using Payment.Services.Infra.Database;
using Payment.Services.Worker.Workers;

namespace Payment.Services.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddInfrastructure();
            builder.Services.AddHostedService<KafkaConsumerWorker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
