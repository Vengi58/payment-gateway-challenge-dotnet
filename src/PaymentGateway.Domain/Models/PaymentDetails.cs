namespace PaymentGateway.Domain.Models
{
    public record PaymentDetails(Guid? Id, string Currency, int Amount) { }
}
