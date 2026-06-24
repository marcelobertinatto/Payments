using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Middleware.Exceptions
{
    public class DuplicateRequestException : AppException
    {
        public DuplicateRequestException(string idempotencyKey) 
            : base($"Request '{idempotencyKey}' already exists.", StatusCodes.Status409Conflict)
        {
        }
    }
}
