namespace Payment.Services.Domain.Interfaces
{
    public interface IEventBus
    {
        Task PublishAsync(
        string topic,
        string key,
        object message);
    }
}
