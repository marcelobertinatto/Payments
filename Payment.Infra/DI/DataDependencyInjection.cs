using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Payment.Services.Domain.Interfaces;
using Payment.Services.Infra.Messaging;
using Payment.Services.Infra.Repositories;

namespace Payment.Services.Infra.Database
{
    public static class DataDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IAmazonDynamoDB>(_ =>
            {
                var config =
                    new AmazonDynamoDBConfig
                    {
                        ServiceURL =
                            "http://localhost:8000"
                    };

                return new AmazonDynamoDBClient(config);
            });

            services.AddScoped<IPaymentRepository,DynamoDbPaymentRepository>();
            services.AddSingleton<IEventBus, KafkaEventBus>();

            return services;
        }
    }
}
