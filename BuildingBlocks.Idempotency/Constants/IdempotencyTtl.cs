namespace BuildingBlocks.Idempotency.Constants
{
    public static class IdempotencyTtl
    {
        public static readonly TimeSpan PaymentRequest = TimeSpan.FromDays(1);

        public static readonly TimeSpan EventProcessing = TimeSpan.FromDays(7);
    }
}
