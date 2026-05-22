using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Contracts
{
    public record PaymentCompletedEvent(string PaymentId, string CorrelationId);
}
