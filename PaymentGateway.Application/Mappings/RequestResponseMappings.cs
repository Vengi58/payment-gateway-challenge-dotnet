using PaymentGateway.Application.Payments.Commands.CreatePayment;
using PaymentGateway.Application.Payments.Queries.GetPayment;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Mappings
{
    internal static class RequestResponseMappings
    {
        public static CreatePaymentCommandResponse MapToCreateCreatePaymentResponse(this (Guid paymentId, BankPaymentStatus status, CardDetails cardDetails, PaymentDetails paymentDetails) details, Func<byte[], string> decrypt)
        {
            return new(
                details.paymentId,
                details.status,
                decrypt(details.cardDetails.CardNumberLastFourDigits),
                details.cardDetails.ExpiryMonth,
                details.cardDetails.ExpiryYear,
                details.paymentDetails.Currency,
                details.paymentDetails.Amount
                );
        }
        public static GetPaymentQueryResponse MapToCreateGetPaymentResponse(this (Guid paymentId, BankPaymentStatus status, CardDetails cardDetails, PaymentDetails paymentDetails) details, Func<byte[], string> decrypt)
        {
            return new(
                details.paymentId,
                details.status,
                decrypt(details.cardDetails.CardNumberLastFourDigits),
                details.cardDetails.ExpiryMonth,
                details.cardDetails.ExpiryYear,
                details.paymentDetails.Currency,
                details.paymentDetails.Amount
                );
        }

        public static CardDetails MapToCardDetails(this CreatePaymentCommand createPaymentCommand, Func<string, byte[]> encrypt)
        {
            return new(encrypt(createPaymentCommand.CardNumber), createPaymentCommand.ExpiryYear, createPaymentCommand.ExpiryMonth, encrypt(createPaymentCommand.Cvv.ToString()));
        }
        public static PaymentDetails MapToPaymentDetails(this CreatePaymentCommand createPaymentCommand)
        {
            return new(createPaymentCommand.PaymentId, createPaymentCommand.Currency, createPaymentCommand.Amount);
        }
        public static BankCardDetails MapToBankCardDetails(this CreatePaymentCommand createPaymentCommand)
        {
            return new(createPaymentCommand.CardNumber, createPaymentCommand.ExpiryYear, createPaymentCommand.ExpiryMonth, createPaymentCommand.Cvv.ToString());
        }
    }
}
