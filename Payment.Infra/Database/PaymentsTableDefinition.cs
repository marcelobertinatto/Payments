using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Payment.Services.Infra.Database.Interface;

namespace Payment.Services.Infra.Database
{
    public class PaymentsTableDefinition : IDynamoDbTableDefinition
    {
        public string TableName => "Payments";

        public CreateTableRequest GetTableRequest() => new()
        {
            TableName = TableName,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            AttributeDefinitions = [new("Id", ScalarAttributeType.S)],
            KeySchema = [new("Id", KeyType.HASH)]
        };
    }
}
