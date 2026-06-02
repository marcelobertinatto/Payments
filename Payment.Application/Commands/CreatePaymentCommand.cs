using BuildingBlocks.Contracts;

namespace Payment.Services.Application.Commands
{
    public record CreatePaymentCommand(decimal Amount, string Currency, string CustomerEmail, string CorrelationId);
}
