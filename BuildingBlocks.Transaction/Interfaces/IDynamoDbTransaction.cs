namespace BuildingBlocks.Transaction.Interfaces
{
    public interface IDynamoDbTransaction
    {
        void AddPut<T>(T entity);

        void AddConditionalPut<T>(T entity, string conditionExpression);

        Task CommitAsync(CancellationToken cancellationToken);
    }
}
