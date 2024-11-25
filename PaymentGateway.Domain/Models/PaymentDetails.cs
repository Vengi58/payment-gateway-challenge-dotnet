namespace PaymentGateway.Domain.Models
{
    public class PaymentDetails
    {
        public Guid? Id { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
    }
}
