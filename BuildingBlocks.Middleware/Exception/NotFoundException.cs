using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Middleware.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, StatusCodes.Status404NotFound)
        {
        }
    }
}
