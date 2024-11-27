using MediatR;

using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public sealed record CreatePaymentCommand(Guid? PaymentId, string Currency, int Amount, string CardNumber, int ExpiryMonth, int ExpiryYear, int Cvv, Merchant Merchant) : IRequest<CreatePaymentCommandResponse> { }
}
