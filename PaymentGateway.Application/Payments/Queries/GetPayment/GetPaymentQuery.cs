using MediatR;

namespace PaymentGateway.Application.Payments.Queries.GetPayment
{
    public sealed record GetPaymentQuery(Guid PaymentId) : IRequest<GetPaymentResponse> { }
}
