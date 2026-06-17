using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Payment.Services.Infra.Database.Interface;

namespace Payment.Services.Infra.Database
{
    public class IdempotencyTableDefinition : IDynamoDbTableDefinition
    {
        public string TableName => "IdempotencyRecords";

        public CreateTableRequest GetTableRequest() => new()
        {
            TableName = TableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = [new("IdempotencyKey", ScalarAttributeType.S)],
            KeySchema = [new("IdempotencyKey", KeyType.HASH)]
        };
    }
}
