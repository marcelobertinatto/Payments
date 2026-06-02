using Amazon.DynamoDBv2.DataModel;

namespace BuildingBlocks.Messaging.Persistence.Model
{
    [DynamoDBTable("ProcessedEvents")]
    public class ProcessedEvent
    {
        [DynamoDBHashKey]
        public string EventId { get; set; }
        public string ProcessedAtUtc { get; set; }
    }
}
