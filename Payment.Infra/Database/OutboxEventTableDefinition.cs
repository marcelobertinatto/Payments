using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Payment.Services.Infra.Database.Interface;

namespace Payment.Services.Infra.Database
{
    public class OutboxEventTableDefinition : IDynamoDbTableDefinition
    {
        public string TableName => "OutboxEvents";

        public CreateTableRequest GetTableRequest() => new()
        {
            TableName = TableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = [new("EventId", ScalarAttributeType.S)],
            KeySchema = [new("EventId", KeyType.HASH)]
        };
    }
}
