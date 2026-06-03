using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Middleware.Exceptions
{
    public sealed class RepositoryException : AppException
    {
        public RepositoryException(string message) : base(message, StatusCodes.Status500InternalServerError)
        {
        }
    }
}
