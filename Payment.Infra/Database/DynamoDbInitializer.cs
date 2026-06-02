using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Payment.Services.Infra.Database.Interface;

namespace Payment.Services.Infra.Database
{
    public class DynamoDbInitializer
    {
        private readonly IAmazonDynamoDB _client;
        private readonly IEnumerable<IDynamoDbTableDefinition> _tableDefinitions;

        public DynamoDbInitializer(IAmazonDynamoDB client, 
            IEnumerable<IDynamoDbTableDefinition> tableDefinitions)
        {
            _client = client;
            _tableDefinitions = tableDefinitions;
        }

        public async Task InitializeAsync()
        {
            var tables = await _client.ListTablesAsync();
            var existingTables = tables.TableNames.ToHashSet();

            var tablesToCreate = _tableDefinitions
            .Where(def => !existingTables.Contains(def.TableName))
            .ToList();

            var creationTasks = tablesToCreate.Select(async def =>
            {
                var request = def.GetTableRequest();
                await _client.CreateTableAsync(request);
                await WaitForTableAsync(def.TableName);
                Console.WriteLine($"Table {def.TableName} successfully initialized.");
            });

            await Task.WhenAll(creationTasks);
        }

        private async Task WaitForTableAsync(string tableName)
        {
            // 1. Create a token that triggers automatically after 30 seconds
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            while (true)
            {
                // 2. If 30 seconds passed, this will throw an OperationCanceledException and break the loop
                cts.Token.ThrowIfCancellationRequested();
                var response = await _client.DescribeTableAsync(tableName, cts.Token);
                if (response.Table.TableStatus == TableStatus.ACTIVE) break;
                await Task.Delay(1000, cts.Token);
            }
        }
    }
}
