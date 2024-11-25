namespace PaymentGateway.Persistance.Entities
{
    internal record Payment(Guid Id, byte[] CardNumber, int ExpiryYear, int ExpiryMonth, byte[] Cvv, string Currency, int Amount, PaymentStatus PaymentStatus) { }
}
