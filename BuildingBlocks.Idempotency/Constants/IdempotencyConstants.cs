namespace BuildingBlocks.Idempotency.Constants
{
    public static class IdempotencyConstants
    {
        public const string PendingReferenceId = "PENDING";

        public const int MaxPollingAttempts = 10;

        public const int PollingDelayMilliseconds = 200;
    }
}
