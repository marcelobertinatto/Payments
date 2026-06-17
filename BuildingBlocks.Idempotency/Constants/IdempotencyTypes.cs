namespace BuildingBlocks.Idempotency.Constants
{
    public static class IdempotencyTypes
    {
        public const string PaymentRequest = nameof(PaymentRequest);

        public const string PaymentCreatedEvent = nameof(PaymentCreatedEvent);

        public const string PaymentCompletedEvent = nameof(PaymentCompletedEvent);

        public const string NotificationEvent = nameof(NotificationEvent);
    }
}
