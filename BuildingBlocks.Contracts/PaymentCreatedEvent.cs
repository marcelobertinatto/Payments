namespace BuildingBlocks.Contracts
{
    public record PaymentCreatedEvent(string id, string correlationId, decimal amount, string currency, string customerEmail, string status) : IntegrationEvent;
}
