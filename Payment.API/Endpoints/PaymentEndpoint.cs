using Microsoft.AspNetCore.Mvc;
using Payment.Services.API.Mappings;
using Payment.Services.API.Request;
using Payment.Services.API.Response;
using Payment.Services.Application.Handlers;


namespace Payment.Services.API.Endpoints
{
    public static class PaymentEndpoint
    {
        public static void MapPaymentEndpoint(this WebApplication app)
        {
            app.MapPost("/api/payments", async (HttpContext context, PaymentRequest paymentRequest, [FromServices] CreatePaymentHandler createPaymentHandler) =>
            {
                var correlationId = context.Items["CorrelationId"]!.ToString()!;

                var command = paymentRequest.ToCommand(correlationId);

                var paymentId = await createPaymentHandler.Handle(command);

                return Results.Ok(new PaymentResponse
                {
                    PaymentId = paymentId,
                    CorrelationId = correlationId
                });
            });
        }
    }
}
