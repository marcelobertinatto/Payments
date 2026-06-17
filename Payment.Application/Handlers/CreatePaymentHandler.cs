using BuildingBlocks.Contracts;
using BuildingBlocks.Idempotency.Constants;
using BuildingBlocks.Idempotency.Services.Interfaces;
using BuildingBlocks.Messaging.Persistence.Model;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;
using BuildingBlocks.Middleware.Exceptions;
using Payment.Services.Application.Commands;
using Payment.Services.Application.Mappings;
using Payment.Services.Domain.Interfaces;
using System.Text.Json;

namespace Payment.Services.Application.Handlers
{
    public class CreatePaymentHandler
    {
        private readonly IPaymentRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IIdempotencyService _idempotencyService;

        public CreatePaymentHandler(IPaymentRepository repo, IEventBus eventBus, IOutboxRepository outboxRepository, IIdempotencyService idempotencyService)
        {
            _repo = repo;
            _eventBus = eventBus;
            _outboxRepository = outboxRepository;
            _idempotencyService = idempotencyService;
        }

        public async Task<string> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            var payment = Domain.Model.Payment.Create(command.Amount, command.Currency, command.CustomerEmail, command.CorrelationId);

            //this method is used to handle the case when the same request is sent multiple times with the same idempotency key. It will wait for the existing payment to be created and return its ID instead of creating a new one.
            var acquired = await _idempotencyService.TryAcquireAsync(command.IdempotencyKey, IdempotencyTypes.PaymentRequest, IdempotencyConstants.PendingReferenceId, IdempotencyTtl.PaymentRequest,cancellationToken);

            if (!acquired)
            {
                //this method is used to wait for the existing payment to be created by polling the idempotency record until it has a reference id or a timeout occurs
                return await WaitForExistingPaymentAsync(command.IdempotencyKey, cancellationToken);
            }

            //saving payment into Payment table in DynamoDB
            await _repo.SaveAsync(payment);

            //it's used for updating the PENDING state for Payment.Id in Idempotency table to avoid multiple creation of the same payment for the same idempotency key
            await _idempotencyService.UpdateReferenceIdAsync(command.IdempotencyKey, payment.Id, cancellationToken);

            var createdEvent = payment.ToCreatedEvent(command.CorrelationId);

            //this outbox pattern is used to ensure that the event is published only after the payment is successfully saved in the database. The event will be stored in the outbox table and then published by a background service.
            await _outboxRepository.SaveAsync(
                new OutboxEvent
                {
                    EventId = createdEvent.EventId.ToString(),

                    EventType = nameof(PaymentCreatedEvent),

                    Payload = JsonSerializer.Serialize(createdEvent),

                    Status = "Pending",

                    CreatedAtUtc = DateTime.UtcNow.ToString("O")
                });

            return payment.Id;
        }

        private async Task<string> WaitForExistingPaymentAsync(string idempotencyKey, CancellationToken cancellationToken)
        {
            for (int attempt = 0; attempt < IdempotencyConstants.MaxPollingAttempts; attempt++)
            {
                var record = await _idempotencyService.GetAsync(idempotencyKey, cancellationToken);

                if (record is not null && record.ReferenceId != IdempotencyConstants.PendingReferenceId)
                {
                    return record.ReferenceId;
                }

                await Task.Delay(IdempotencyConstants.PollingDelayMilliseconds, cancellationToken);
            }

            throw new AppException($"Payment creation is still in progress for idempotency key '{idempotencyKey}'.");
        }
    }
}
