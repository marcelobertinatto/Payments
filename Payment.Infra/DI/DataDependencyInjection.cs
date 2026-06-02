using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Services.Domain.Interfaces;
using Payment.Services.Infra.Messaging;
using Payment.Services.Infra.Repositories;
using System.Net;

namespace Payment.Services.Infra.Database
{
    public static class DataDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonDynamoDB>(sp =>
            {
                // 1. Obtém o IConfiguration do container de DI
                var configuration = sp.GetRequiredService<IConfiguration>();

                // 2. Lê a ServiceUrl definida no seu appsettings.json
                var serviceUrl = configuration["DynamoDb:ServiceUrl"];

                // 3. Configura o SDK para apontar para o Docker em vez da nuvem
                var config = new AmazonDynamoDBConfig
                {
                    ServiceURL = serviceUrl,
                    Timeout = TimeSpan.FromSeconds(5),
                    MaxErrorRetry = 1
                };

                var credentials = new BasicAWSCredentials("local", "local");

                return new AmazonDynamoDBClient(credentials, config);
            });

            services.AddSingleton<IDynamoDBContext>(sp =>
            {
                var client = sp.GetRequiredService<IAmazonDynamoDB>();
                return new DynamoDBContextBuilder()
                    .WithDynamoDBClient(() => client)
                    .Build();
            });

            services.AddScoped<IPaymentRepository,DynamoDbPaymentRepository>();
            services.AddScoped<IProcessedEventRepository,DynamoDbProcessedEventRepository>();
            services.AddSingleton<IEventBus, KafkaEventBus>();

            return services;
        }
    }
}
