using MediatR;

using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Commands
{
    public sealed record CreatePaymentCommand(string Currency, int Amount,string CardNumber, int ExpiryMonth, int ExpiryYear, int Cvv) : IRequest<PostPaymentResponse>
    {
    }
}
