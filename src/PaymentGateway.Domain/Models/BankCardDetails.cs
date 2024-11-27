namespace PaymentGateway.Domain.Models
{
    public record BankCardDetails(string CardNumber, int ExpiryYear, int ExpiryMonth, string Cvv) { }
}