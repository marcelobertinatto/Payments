using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Middleware.Exceptions
{
    public class EventBusException : AppException
    {
        public EventBusException(string message) : base(message, StatusCodes.Status503ServiceUnavailable)
        {
        }
    }
}
