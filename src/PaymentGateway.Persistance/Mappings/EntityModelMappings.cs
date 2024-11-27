using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;

namespace PaymentGateway.Persistance.Mappings
{
    internal static class EntityModelMappings
    {
        public static Payment MapToPayment(this (CardDetails cardDetails, PaymentDetails paymentDetails, PaymentProcessingStatus paymentStatus) details)
        {
            return new(
            details.paymentDetails.Id ?? Guid.NewGuid(),
            details.cardDetails.CardNumberLastFourDigits,
            details.cardDetails.ExpiryYear, 
            details.cardDetails.ExpiryMonth,
            details.paymentDetails.Currency,
            details.paymentDetails.Amount,
            details.paymentStatus);
        }

        public static PaymentDetails MapToPaymentDetails(this Payment payment)
        {
            return new(payment.Id, payment.Currency, payment.Amount);
        }

        public static CardDetails MapToCardDetails(this Payment payment)
        {
            return new(
                payment.CardNumberLastFour,
                payment.ExpiryYear,
                payment.ExpiryMonth,
                default);
        }

        public static PaymentProcessingStatus MapToPaymentStatus(this BankPaymentStatus bankPaymentStatus)
        {
            switch (bankPaymentStatus)
            {
                case BankPaymentStatus.Authorized:
                    return PaymentProcessingStatus.FinishedProcessing;
                case BankPaymentStatus.Declined:
                case BankPaymentStatus.Rejected:
                    return PaymentProcessingStatus.FailedProcessing;
                default:
                    return PaymentProcessingStatus.Processing;
            }
        }

        public static BankPaymentStatus MapToBankPaymentStatus(this PaymentProcessingStatus paymentStatus)
        {
            switch (paymentStatus)
            {
                case PaymentProcessingStatus.Processing:
                case PaymentProcessingStatus.FinishedProcessing:
                    return BankPaymentStatus.Authorized;
                case PaymentProcessingStatus.FailedProcessing:
                    return BankPaymentStatus.Declined;
                default:
                    return BankPaymentStatus.Rejected;
            }
        }
    }
}
