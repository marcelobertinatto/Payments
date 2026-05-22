using Payment.Services.Application.Handlers;

namespace Payment.Services.API.DI
{
    public static class ApplicationDependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CreatePaymentHandler>();

            return services;
        }
    }
}
