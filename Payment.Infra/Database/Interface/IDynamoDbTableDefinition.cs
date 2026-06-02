using Amazon.DynamoDBv2.Model;

namespace Payment.Services.Infra.Database.Interface
{
    public interface IDynamoDbTableDefinition
    {
        string TableName { get; }
        CreateTableRequest GetTableRequest();
    }
}
