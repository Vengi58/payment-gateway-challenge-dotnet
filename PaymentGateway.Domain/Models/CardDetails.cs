namespace PaymentGateway.Domain.Models
{
    public record CardDetails(byte[] CardNumberLastFourDigits, int ExpiryYear, int ExpiryMonth, byte[] Cvv) { }
}