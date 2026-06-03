using RabbitMQ.Client;
using System.Text;

namespace Payment.Services.Notification.Worker.Helper
{
    public static class RabbitMqRetryHelper
    {
        public static int GetRetryCount(IReadOnlyBasicProperties properties)
        {
            if (properties.Headers is null)
                return 0;

            if (!properties.Headers.TryGetValue("x-retry-count", out var value))
                return 0;

            return Convert.ToInt32(Encoding.UTF8.GetString((byte[])value));
        }

        public static IDictionary<string, object> CreateRetryHeaders(int retryCount)
        {
            return new Dictionary<string, object>
            {
                ["x-retry-count"] = Encoding.UTF8.GetBytes(retryCount.ToString())
            };
        }
    }
}
