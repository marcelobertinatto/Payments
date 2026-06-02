namespace BuildingBlocks.Middleware.Model
{
    public class ErrorResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
