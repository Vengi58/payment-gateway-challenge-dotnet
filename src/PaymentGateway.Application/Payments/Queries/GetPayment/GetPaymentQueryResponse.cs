using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Payments.Queries.GetPayment
{
    public sealed record GetPaymentQueryResponse(Guid PaymentId, BankPaymentStatus Status, string CardNumberLastFourDigits, int ExpiryMonth, int ExpiryYear, string Currency, int Amount) { }
}
