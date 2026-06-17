using Payment.Services.API.Request;
using Payment.Services.Application.Commands;
using System.Reflection.Metadata;

namespace Payment.Services.API.Mappings
{
    public static class PaymentApiMappings
    {
        public static CreatePaymentCommand ToCommand(this PaymentRequest request, string correlationId, string idempotencyKey)
        => new CreatePaymentCommand(request.Amount, request.Currency, request.CustomerEmail, correlationId, idempotencyKey);
    }
}
