namespace Payment.Services.Domain.Interfaces
{
    public interface IEventBus
    {
        Task PublishAsync<T>(string topic,string key,T message);

        public Task PublishRawAsync(string topic, string key, string json);
    }
}
