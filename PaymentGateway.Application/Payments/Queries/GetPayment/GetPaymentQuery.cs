using MediatR;

using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Payments.Queries.GetPayment
{
    public sealed record GetPaymentQuery(Guid PaymentId, Merchant Merchant) : IRequest<GetPaymentQueryResponse> { }
}
