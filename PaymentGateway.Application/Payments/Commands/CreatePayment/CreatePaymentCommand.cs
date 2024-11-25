using MediatR;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public sealed record CreatePaymentCommand(Guid? Id, string Currency, int Amount, string CardNumber, int ExpiryMonth, int ExpiryYear, int Cvv) : IRequest<CreatePaymentResponse> { }
}
