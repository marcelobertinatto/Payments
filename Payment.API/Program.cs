
using BuildingBlocks.Middleware;
using Payment.Services.API.DI;
using Payment.Services.API.Endpoints;
using Payment.Services.Infra.Database;
using Scalar.AspNetCore;

namespace Payment.Services.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddSingleton<KafkaProducer>();
            //builder.Services.AddSingleton<DynamoDbRepository>();

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure();

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            app.UseMiddleware<CorrelationIdMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference(options =>
                {
                    options.WithTitle("Scalar Example API")
                        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapPaymentEndpoint();

            app.Run();
        }
    }
}
