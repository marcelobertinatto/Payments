using Microsoft.AspNetCore.Mvc;
using Payment.Services.API.Mappings;
using Payment.Services.API.Request;
using Payment.Services.API.Response;
using Payment.Services.Application.Handlers;
using System.ComponentModel.DataAnnotations;


namespace Payment.Services.API.Endpoints
{
    public static class PaymentEndpoint
    {
        public static void MapPaymentEndpoint(this WebApplication app)
        {
            app.MapPost("/api/payments", async (HttpContext context, PaymentRequest paymentRequest, [FromServices] CreatePaymentHandler createPaymentHandler, CancellationToken cancellationToken) =>
            {
                var correlationId = context.Items["CorrelationId"]!.ToString()!;
                var idempotencyKey = context.Request.Headers["X-Idempotency-Key"]
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    throw new ValidationException("X-Idempotency-Key header is required.");
                }

                var command = paymentRequest.ToCommand(correlationId, idempotencyKey);

                var paymentId = await createPaymentHandler.Handle(command, cancellationToken);

                return Results.Ok(new PaymentResponse
                {
                    PaymentId = paymentId,
                    CorrelationId = correlationId
                });
            });
        }
    }
}
