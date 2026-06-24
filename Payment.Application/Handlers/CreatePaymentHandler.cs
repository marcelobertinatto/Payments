using Amazon.DynamoDBv2.Model;
using BuildingBlocks.Contracts;
using BuildingBlocks.Idempotency.Constants;
using BuildingBlocks.Idempotency.Models;
using BuildingBlocks.Idempotency.Services.Interfaces;
using BuildingBlocks.Messaging.Persistence.Model;
using BuildingBlocks.Messaging.Persistence.Repository.Interface;
using BuildingBlocks.Middleware.Exceptions;
using BuildingBlocks.Transaction.Interfaces;
using Payment.Services.Application.Commands;
using Payment.Services.Application.Mappings;
using Payment.Services.Domain.Interfaces;
using Payment.Services.Domain.Model;
using System.Text.Json;

namespace Payment.Services.Application.Handlers
{
    public class CreatePaymentHandler
    {
        private readonly IPaymentRepository _repo;
        private readonly IEventBus _eventBus;
        private readonly IOutboxRepository _outboxRepository;
        private readonly IIdempotencyService _idempotencyService;
        private readonly IDynamoDbTransaction _transaction;

        public CreatePaymentHandler(IPaymentRepository repo, IEventBus eventBus, IOutboxRepository outboxRepository, IIdempotencyService idempotencyService, IDynamoDbTransaction transaction)
        {
            _repo = repo;
            _eventBus = eventBus;
            _outboxRepository = outboxRepository;
            _idempotencyService = idempotencyService;
            _transaction = transaction;
        }

        public async Task<string> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var existing = await _idempotencyService.GetAsync(command.IdempotencyKey, cancellationToken);

                if (existing == null)
                {

                    var payment = Domain.Model.Payment.Create(command.Amount, command.Currency, command.CustomerEmail, command.CorrelationId);

                    //this is used for creating the objects to be used into a single transaction.
                    //it will be guaranteed that either all the operations will be successful or none of them will be applied.
                    IdempotencyRecord idempotencyRecord = CreateIdempotencyRecord(command.IdempotencyKey);
                    PaymentCreatedEvent createdEvent = payment.ToCreatedEvent(command.CorrelationId);
                    OutboxEvent outboxEvent = CreateOutboxEvent(createdEvent, payment.Id);

                    //used for adding a generic object to be used in the transaction, it will be used for the payment and the outbox event.
                    _transaction.AddPut(payment);
                    _transaction.AddPut(outboxEvent);
                    //this part is used for checking the idempotency record, if it already exists, it will throw an exception, if not, it will be added to the transaction.
                    _transaction.AddConditionalPut(idempotencyRecord, "attribute_not_exists(IdempotencyKey)");

                    //execute the transaction, if any of the operations fail, none of them will be applied into DynamoDB.
                    await _transaction.CommitAsync(cancellationToken);

                    //after saved, return the payment id to the caller.
                    return payment.Id;
                }
                else
                {
                    return existing.ReferenceId;
                }
            }
            catch (TransactionCanceledException)
            {                
                throw new DuplicateRequestException(command.IdempotencyKey);                
            }
        }

        private IdempotencyRecord CreateIdempotencyRecord(string idempotencyKey)
        {
            return new IdempotencyRecord
            {
                IdempotencyKey =  idempotencyKey,
                Type = IdempotencyTypes.PaymentRequest,
                Status = IdempotencyStatus.Completed,
                ReferenceId = IdempotencyConstants.PendingReferenceId,
                CreatedAtUtc = DateTime.UtcNow.ToString("O"),
                UpdatedAtUtc = DateTime.UtcNow.ToString("O"),
                ExpiresAt = DateTimeOffset.UtcNow .AddDays(1) .ToUnixTimeSeconds()
            };
        }

        private OutboxEvent CreateOutboxEvent(PaymentCreatedEvent paymentCreatedEvent, string paymentId)
        {
            return new OutboxEvent
            {
                EventId = paymentCreatedEvent.EventId.ToString(),
                AggregateId = paymentId,
                Topic = "payments-created",
                Payload = JsonSerializer.Serialize(paymentCreatedEvent),
                Status = OutboxStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow.ToString("O")
            };
        }
    }
}
