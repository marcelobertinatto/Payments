using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using BuildingBlocks.Transaction.Interfaces;

namespace BuildingBlocks.Transaction.DynamoDB
{
    public class DynamoDbTransaction : IDynamoDbTransaction
    {
        private readonly IDynamoDBContext _context;
        private readonly IAmazonDynamoDB _client;

        private readonly List<TransactWriteItem> _items = [];

        public DynamoDbTransaction(IDynamoDBContext context, IAmazonDynamoDB client)
        {
            _context = context;
            _client = client;
        }

        public void AddConditionalPut<T>(T entity, string conditionExpression)
        {
            var document = _context.ToDocument(entity);

            var tableName = _context.GetTargetTable<T>().TableName;

            _items.Add(new TransactWriteItem
            {
                Put = new Put
                {
                    TableName = tableName,
                    Item = document.ToAttributeMap(),
                    ConditionExpression = conditionExpression
                }
            });
        }

        public void AddPut<T>(T entity)
        {
            var document = _context.ToDocument(entity);

            var tableName = _context.GetTargetTable<T>().TableName;

            _items.Add(
                new TransactWriteItem
                {
                    Put = new Put
                    {
                        TableName = tableName,
                        Item = document.ToAttributeMap()
                    }
                });
        }

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (_items.Count == 0)
            {
                return;
            }

            var request = new TransactWriteItemsRequest
            {
                TransactItems = _items
            };

            await _client.TransactWriteItemsAsync(request, cancellationToken);

            _items.Clear();
        }
    }
}
