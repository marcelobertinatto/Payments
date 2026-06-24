using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingBlocks.Idempotency.Constants
{
    public static class OutboxStatus
    {
        public const string Pending = "Pending";
        public const string Published = "Published";
    }
}
