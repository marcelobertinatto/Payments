using BuildingBlocks.Contracts;
using BuildingBlocks.Logging;
using Payment.Services.Notification.Infrastructure.Setup;
using Payment.Services.Notification.Worker.Helper;
using Payment.Services.NotificationServices.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Payment.Services.Notification.Worker.Workers
{
    public class RabbitMQNotificationProcessorWorker : BackgroundService
    {
        private readonly IChannel _channel;
        private readonly ILogger<RabbitMQNotificationProcessorWorker> _logger;
        private readonly INotificationService _notificationService;
        private const int MaxRetries = 3;

        public RabbitMQNotificationProcessorWorker(IChannel channel, ILogger<RabbitMQNotificationProcessorWorker> logger, INotificationService notificationService)
        {
            _channel = channel;
            _logger = logger;
            _notificationService = notificationService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("RabbitMQ Notification Processor Worker started.");

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();

                        if (body is null || body.Length == 0)
                        {
                            _logger.LogError("Received empty message. Skipping.");
                            return;
                        }

                        var message = Encoding.UTF8.GetString(body);

                        var messageDeserialized = JsonSerializer.Deserialize<PaymentCompletedEvent>(message);

                        //there is a problem deserializing the message. Dont send ACK, so it will be retried
                        if (messageDeserialized is null)
                        {
                            _logger.LogError("Problem deserializing notification message.");

                            await PublishToDlqAsync(ea, 0);

                            await _channel.BasicAckAsync(ea.DeliveryTag,false);

                            return;
                        }

                        //If the message is deserialized successfully, process it
                        using (_logger.BeginEventScope(
                                messageDeserialized.CorrelationId,
                                messageDeserialized.EventId,
                                messageDeserialized.PaymentId))
                        {
                            _logger.LogInformation("Notification message received.");

                            //Responsible for sending the notification to the user
                            //Email, notifications, Whatsapp and etc
                            await _notificationService.SendPaymentCompletedAsync(messageDeserialized, stoppingToken);

                            await _channel.BasicAckAsync(ea.DeliveryTag, false);

                            _logger.LogInformation("Notification processed successfully.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Invalid notification message payload.");

                        //this exception should move to DQL queue immediately, because the message is not in the correct format.
                        await PublishToDlqAsync(ea, 0);

                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected notification processing error.");

                        //this is a generic exception for example:
                        //smtp timeout, database unavailable and etc. In this case, we want to retry the message a few times before moving it to the DLQ.
                        try
                        {
                            var body = Encoding.UTF8.GetString(ea.Body.ToArray());

                            var evt = JsonSerializer.Deserialize<PaymentCompletedEvent>(body);

                            if (evt is not null)
                            {
                                await HandleFailureAsync(ea, evt, ex);
                            }
                            else
                            {
                                await PublishToDlqAsync(ea, 0);

                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            }
                        }
                        catch (Exception retryEx)
                        {
                            _logger.LogCritical(retryEx,"Failed while handling notification failure.");

                            await _channel.BasicNackAsync(ea.DeliveryTag,false,true);
                        }
                    }
                };

                await _channel.BasicConsumeAsync(queue: RabbitMqQueues.Notification, autoAck: false,consumer: consumer);

                _logger.LogInformation("RabbitMQ consumer subscribed to notification-queue.");

                try
                {
                    await Task.Delay(Timeout.Infinite,stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("RabbitMQ Notification Processor Worker stopping.");
                }
            }            
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while processing RabbitMQ messages. Error: {ex.Message}");
            }
        }

        private async Task HandleFailureAsync(BasicDeliverEventArgs ea, PaymentCompletedEvent evt, Exception exception)
        {
            var retryCount = RabbitMqRetryHelper.GetRetryCount(ea.BasicProperties);

            _logger.LogError(exception, "Notification failed. RetryCount={RetryCount}", retryCount);

            if (retryCount >= MaxRetries)
            {
                await PublishToDlqAsync(ea, retryCount);

                await _channel.BasicAckAsync(ea.DeliveryTag,false);

                return;
            }

            await RepublishForRetryAsync(ea,retryCount + 1);

            await _channel.BasicAckAsync(ea.DeliveryTag,false);
        }

        private async Task RepublishForRetryAsync(BasicDeliverEventArgs ea,int retryCount)
        {
            var props =
                new BasicProperties
                {
                    Headers = RabbitMqRetryHelper.CreateRetryHeaders(retryCount)!
                };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: RabbitMqQueues.Notification,
                mandatory: false,
                basicProperties: props,
                body: ea.Body);

            _logger.LogWarning("Message republished. RetryCount={RetryCount}",retryCount);
        }

        private async Task PublishToDlqAsync(BasicDeliverEventArgs ea,int retryCount)
        {
            var props =
                new BasicProperties
                {
                    Headers = RabbitMqRetryHelper.CreateRetryHeaders(retryCount)!
                };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: RabbitMqQueues.NotificationDlq,
                mandatory: false,
                basicProperties: props,
                body: ea.Body);

            _logger.LogError("Message moved to DLQ. RetryCount={RetryCount}",retryCount);
        }
    }
}
