using Payment.Services.OutboxWorker;
using Payment.Services.Infra.Database;

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
