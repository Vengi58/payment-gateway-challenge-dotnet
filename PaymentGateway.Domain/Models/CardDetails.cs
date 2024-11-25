namespace PaymentGateway.Domain.Models
{
    public record CardDetails(string CardNumber, int ExpiryYear, int ExpiryMonth, int Cvv) { }
}