using Amazon.DynamoDBv2.DataModel;

namespace BuildingBlocks.Idempotency.Models
{
    [DynamoDBTable("IdempotencyRecords")]
    public class IdempotencyRecord
    {
        [DynamoDBHashKey]
        public string IdempotencyKey { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string ReferenceId { get; set; } = default!;

        public string CreatedAtUtc { get; set; } = default!;
        public long ExpiresAt { get; set; }
    }
}
