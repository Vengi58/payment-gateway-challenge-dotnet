using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;

namespace PaymentGateway.Persistance.Mappings
{
    internal static class EntityModelMappings
    {
        public static Payment MapToPayment(this (CardDetails cardDetails, PaymentDetails paymentDetails, PaymentStatus paymentStatus) details, Func<string, byte[]> encrypt)
        {
            return new(
            details.paymentDetails.Id ?? Guid.NewGuid(),
            encrypt(details.cardDetails.CardNumber),
            details.cardDetails.ExpiryYear, 
            details.cardDetails.ExpiryMonth,
            encrypt(details.cardDetails.Cvv.ToString()),
            details.paymentDetails.Currency,
            details.paymentDetails.Amount,
            details.paymentStatus);
        }

        public static PaymentDetails MapToPaymentDetails(this Payment payment)
        {
            return new(payment.Id, payment.Currency, payment.Amount);
        }

        public static CardDetails MapToCardDetails(this Payment payment, Func<byte[], string> decrypt)
        {
            return new(
                decrypt(payment.CardNumber),
                payment.ExpiryYear,
                payment.ExpiryMonth,
                Convert.ToInt32(decrypt(payment.Cvv)));
        }

        public static PaymentStatus MapToPaymentStatus(this BankPaymentStatus bankPaymentStatus)
        {
            switch (bankPaymentStatus)
            {
                case BankPaymentStatus.Authorized:
                    return PaymentStatus.Completed;
                case BankPaymentStatus.Declined:
                case BankPaymentStatus.Rejected:
                    return PaymentStatus.Failed;
                default:
                    return PaymentStatus.Created;
            }
        }

        public static BankPaymentStatus MapToBankPaymentStatus(this PaymentStatus paymentStatus)
        {
            switch (paymentStatus)
            {
                case PaymentStatus.Created:
                case PaymentStatus.Completed:
                    return BankPaymentStatus.Authorized;
                case PaymentStatus.Failed:
                    return BankPaymentStatus.Declined;
                default:
                    return BankPaymentStatus.Rejected;
            }
        }
    }
}
