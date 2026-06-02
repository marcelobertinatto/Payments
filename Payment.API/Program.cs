
using BuildingBlocks.Middleware;
using Payment.Services.API.DI;
using Payment.Services.API.Endpoints;
using Payment.Services.Infra.Database;
using Payment.Services.Infra.Database.Interface;
using Scalar.AspNetCore;

namespace Payment.Services.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //builder.Services.AddSingleton<KafkaProducer>();
            //builder.Services.AddSingleton<DynamoDbRepository>();

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure();

            var assembly = typeof(DynamoDbInitializer).Assembly;
            var definitions = assembly.GetTypes()
                .Where(t => typeof(IDynamoDbTableDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in definitions)
            {
                builder.Services.AddSingleton(typeof(IDynamoDbTableDefinition), type);
            }

            builder.Services.AddSingleton<DynamoDbInitializer>();

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<DynamoDbInitializer>();

                // Call the method that reads your IDynamoDbTableDefinition list and creates the tables
                // Note: If your method name is different (e.g., CreateTablesAsync), change it here
                await initializer.InitializeAsync();
            }

            app.UseMiddleware<GlobalExceptionMiddleware>();
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

            await app.RunAsync();
        }
    }
}
