namespace BuildingBlocks.Contracts
{
    public record PaymentCompletedEvent(string PaymentId, string CorrelationId) : IntegrationEvent;
}
