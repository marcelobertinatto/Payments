using Payment.Services.Application.Commands;
using Payment.Services.Application.Mappings;
using Payment.Services.Domain.Interfaces;

namespace Payment.Services.Application.Handlers
{
    public class CreatePaymentHandler
    {
        private readonly IPaymentRepository _repo;
        private readonly IEventBus _eventBus;

        public CreatePaymentHandler(IPaymentRepository repo, IEventBus eventBus)
        {
            _repo = repo;
            _eventBus = eventBus;
        }

        public async Task<string> Handle(CreatePaymentCommand command)
        {
            var payment = Domain.Model.Payment.Create(command.Amount, command.Currency, command.CustomerEmail, command.CorrelationId);

            await _repo.SaveAsync(payment);

            await _eventBus.PublishAsync(
                "payments-created",
                payment.Id,
                payment.ToCreatedEvent(command.CorrelationId));

            return payment.Id;
        }
    }
}
