using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Payments.Commands.CreatePayment;

namespace PaymentGateway.Api.Extensions
{
    public static class RequestResponseExtensions
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


        public static PostPaymentResponse MapToPostPaymentResponse(this PostPaymentRequest postPaymentRequest, CreatePaymentResponse createPaymentResponse)
        {
            return new PostPaymentResponse()
            {
                Id = createPaymentResponse.PaymentId,
                Status = createPaymentResponse.Status,
                Amount = postPaymentRequest.Amount,
                CardNumberLastFour = postPaymentRequest.CardNumber.Substring(postPaymentRequest.CardNumber.Length - 5, 4),
                Currency = postPaymentRequest.Currency,
                ExpiryMonth = postPaymentRequest.ExpiryMonth,
                ExpiryYear = postPaymentRequest.ExpiryYear
            };
        }
    }
}
