namespace BuildingBlocks.Contracts
{
    public abstract record IntegrationEvent
    {
        public Guid EventId { get; init; }

        public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;

        protected IntegrationEvent() => EventId = Guid.NewGuid();

        protected IntegrationEvent(Guid eventId) => EventId = eventId;
    }
}
