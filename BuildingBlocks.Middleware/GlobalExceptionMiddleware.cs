using BuildingBlocks.Middleware.Exceptions;
using BuildingBlocks.Middleware.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BuildingBlocks.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context,ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception,"Unhandled exception");

            var correlationId = context.Items["CorrelationId"]?.ToString()
                ?? Guid.NewGuid().ToString();

            var response = new ErrorResponse
            {
                CorrelationId = correlationId,
                Message = GetFriendlyMessage(exception)
            };

            context.Response.ContentType = "application/json";

            context.Response.StatusCode = GetStatusCode(exception);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,

                NotFoundException => StatusCodes.Status404NotFound,

                EventBusException => StatusCodes.Status503ServiceUnavailable,

                RepositoryException => StatusCodes.Status500InternalServerError,

                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static string GetFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ValidationException => exception.Message,

                NotFoundException => exception.Message,

                EventBusException =>
                    "Payment was created but the messaging infrastructure is currently unavailable.",

                RepositoryException =>
                    "An error occurred while saving data.",

                _ =>
                    "An unexpected error occurred. Please try again later."
            };
        }
    }
}
