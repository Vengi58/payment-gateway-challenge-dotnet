using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public sealed record CreatePaymentResponse(Guid PaymentId, BankPaymentStatus Status, string CardNumberLastFourDigits, int ExpiryMonth, int ExpiryYear, string Currency, int Amount) { }
}
