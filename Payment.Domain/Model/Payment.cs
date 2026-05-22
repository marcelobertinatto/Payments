namespace Payment.Services.Domain.Model
{
    public class Payment
    {
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime PaidAt { get; set; }
        public string Status { get; set; }       
        public void Complete() => Status = "COMPLETED";

        public static Payment Create(decimal amount, string currency, string customerEmail, string correlationId)
        {
            return new Payment
            {
                Id = Guid.NewGuid().ToString(),
                Amount = amount,
                Currency = currency,
                CustomerEmail = customerEmail,
                CorrelationId = correlationId,
                Status = "CREATED"
            };
        }

        public static Payment Restore(string id, decimal amount, string currency, string customerEmail, string status, string correlationId)
        {
            return new Payment
            {
                Id = id,
                Amount = amount,
                Currency = currency,
                CustomerEmail = customerEmail,
                CorrelationId = correlationId,
                Status = status
            };
        }
    }
}
