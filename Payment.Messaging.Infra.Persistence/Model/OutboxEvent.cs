using Amazon.DynamoDBv2.DataModel;

namespace BuildingBlocks.Messaging.Persistence.Model
{
    [DynamoDBTable("OutboxEvents")]
    public class OutboxEvent
    {
        [DynamoDBHashKey]
        public string EventId { get; set; } = default!;

        public string AggregateId { get; set; } = default!;

        public string Topic { get; set; } = default!;

        public string EventType { get; set; } = default!;

        public string Payload { get; set; } = default!;

        public string Status { get; set; } = "Pending";

        public string CreatedAtUtc { get; set; } = default!;
    }
}
