namespace PaymentGateway.Persistance.Entities
{
    internal record Payment(Guid Id, byte[] CardNumberLastFour, int ExpiryYear, int ExpiryMonth, string Currency, int Amount, PaymentProcessingStatus PaymentStatus) { }
}
