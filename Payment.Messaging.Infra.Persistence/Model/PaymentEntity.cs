using Amazon.DynamoDBv2.DataModel;

namespace BuildingBlocks.Messaging.Persistence.Model
{
    [DynamoDBTable("Payments")]
    public class PaymentEntity
    {
        [DynamoDBHashKey]
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
