using Payment.Services.Infra.Database;
using Payment.Services.OutboxWorker;

namespace OutboxWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddInfrastructure();
            builder.Services.AddHostedService<OutboxPublisherWorker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
