using PaymentGateway.Application.Payments.Commands.CreatePayment;
using PaymentGateway.Application.Payments.Queries.GetPayment;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Mappings
{
    internal static class RequestResponseMappings
    {
        public static CreatePaymentResponse MapToCreateCreatePaymentResponse(this (Guid paymentId, BankPaymentStatus status, CardDetails cardDetails, PaymentDetails paymentDetails) details)
        {
            return new(
                details.paymentId,
                details.status,
                details.cardDetails.CardNumber,
                details.cardDetails.ExpiryMonth,
                details.cardDetails.ExpiryYear,
                details.paymentDetails.Currency,
                details.paymentDetails.Amount
                );
        }
        public static GetPaymentResponse MapToCreateGetPaymentResponse(this (Guid paymentId, BankPaymentStatus status, CardDetails cardDetails, PaymentDetails paymentDetails) details)
        {
            return new(
                details.paymentId,
                details.status,
                details.cardDetails.CardNumber,
                details.cardDetails.ExpiryMonth,
                details.cardDetails.ExpiryYear,
                details.paymentDetails.Currency,
                details.paymentDetails.Amount
                );
        }

        public static CardDetails MapToCardDetails(this CreatePaymentCommand createPaymentCommand)
        {
            return new(createPaymentCommand.CardNumber, createPaymentCommand.ExpiryYear, createPaymentCommand.ExpiryMonth, createPaymentCommand.Cvv);
        }
        public static PaymentDetails MapToPaymentDetails(this CreatePaymentCommand createPaymentCommand)
        {
            return new(createPaymentCommand.Id, createPaymentCommand.Currency, createPaymentCommand.Amount);
        }
    }
}
