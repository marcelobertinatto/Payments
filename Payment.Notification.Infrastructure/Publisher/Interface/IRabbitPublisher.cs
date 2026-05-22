namespace Payment.Services.Notification.Infrastructure.Publisher.Interface
{
    public interface IRabbitPublisher
    {
        public Task PublishAsync(string queue, string message);
    }
}
