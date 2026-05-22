namespace Payment.Services.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        Task SaveAsync(Model.Payment entity);
        Task<Model.Payment> GetAsync(string paymentId);
    }
}
