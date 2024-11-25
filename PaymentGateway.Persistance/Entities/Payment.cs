namespace PaymentGateway.Persistance.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }
        public byte[] CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public byte[] Cvv { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
    }
}
