using MediatR.NotificationPublishers;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Payments.Commands.CreatePayment;

namespace PaymentGateway.Api.Mappings
{
    public static class RequestResponseMappings
    {
        public static CreatePaymentCommand MapToCreatePaymentCommand(this PostPaymentRequest postPaymentRequest, Guid? idempotencyKey)
        {
            return new CreatePaymentCommand(
                idempotencyKey,
                postPaymentRequest.Currency,
                postPaymentRequest.Amount,
                postPaymentRequest.CardNumber,
                postPaymentRequest.ExpiryMonth,
                postPaymentRequest.ExpiryYear,
                postPaymentRequest.Cvv);
        }


        public static PostPaymentResponse MapToPostPaymentResponse(this CreatePaymentResponse createPaymentResponse)
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

        public static GetPaymentResponse MaptToGetPaymentResponse(this Application.Payments.Queries.GetPayment.GetPaymentResponse getPaymentResponse) {
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
