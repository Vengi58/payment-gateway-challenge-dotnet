using MediatR.NotificationPublishers;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Payments.Commands.CreatePayment;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Api.Mappings
{
    public static class RequestResponseMappings
    {
        public static CreatePaymentCommand MapToCreatePaymentCommand(this (PostPaymentRequest postPaymentRequest, Guid? idempotencyKey, Merchant merchant) details)
        {
            return new CreatePaymentCommand(
                details.idempotencyKey,
                details.postPaymentRequest.Currency,
                details.postPaymentRequest.Amount,
                details.postPaymentRequest.CardNumber,
                details.postPaymentRequest.ExpiryMonth,
                details.postPaymentRequest.ExpiryYear,
                details.postPaymentRequest.Cvv,
                details.merchant);
        }


        public static PostPaymentResponse MapToPostPaymentResponse(this CreatePaymentCommandResponse createPaymentResponse)
        {
            return new PostPaymentResponse()
            {
                Id = createPaymentResponse.PaymentId,
                Status = createPaymentResponse.Status,
                Amount = createPaymentResponse.Amount,
                CardNumberLastFourDigits = createPaymentResponse.CardNumberLastFourDigits,
                Currency = createPaymentResponse.Currency,
                ExpiryMonth = createPaymentResponse.ExpiryMonth,
                ExpiryYear = createPaymentResponse.ExpiryYear
            };
        }

        public static GetPaymentResponse MaptToGetPaymentResponse(this Application.Payments.Queries.GetPayment.GetPaymentQueryResponse getPaymentResponse) {
            return new GetPaymentResponse()
            {
                Status = getPaymentResponse.Status,
                Amount = getPaymentResponse.Amount,
                CardNumberLastFourDigits = getPaymentResponse.CardNumberLastFourDigits,
                Currency = getPaymentResponse.Currency,
                ExpiryMonth = getPaymentResponse.ExpiryMonth,
                ExpiryYear = getPaymentResponse.ExpiryYear,
                Id = getPaymentResponse.PaymentId
            };
        }

    }
}
