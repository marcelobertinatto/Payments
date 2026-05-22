using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Contracts
{
    public record PaymentCreatedEvent(string id, string correlationId, decimal amount, string currency, string customerEmail, string status);
}
