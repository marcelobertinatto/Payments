using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Logging
{
    public static class LoggerScopes
    {
        public static IDisposable BeginEventScope(
        this ILogger logger,
        string correlationId,
        Guid eventId,
        string paymentId)
        {
            return logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["EventId"] = eventId,
                    ["PaymentId"] = paymentId
                });
        }
    }
}
