namespace BuildingBlocks.Messaging.Kafka.Producer.Interface
{
    public interface IKafkaProducer
    {
        Task ProduceAsync<T>(string topic,string key,T message);
    }
}
