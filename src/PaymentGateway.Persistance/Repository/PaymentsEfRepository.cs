using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Repository;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;
using PaymentGateway.Persistance.Mappings;

namespace PaymentGateway.Persistance.Repository
{
    public class PaymentsEfRepository : IPaymentRepository
    {
        private readonly PaymentsDbContext _paymentsDbContext;

        public PaymentsEfRepository(PaymentsDbContext paymentsDbContext)
        {
            _paymentsDbContext = paymentsDbContext;
        }

        public async Task<Guid> CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails, Merchant merchant)
        {
            var merchantEntity = await _paymentsDbContext.Merchants.FindAsync(merchant.MerchantId);
            if (merchantEntity == null) { throw new MerchantNotFoundException($"Merchant {merchant.MerchantId} not found."); }
            PaymentEntity paymentEntity = new()
            {
                Amount = paymentDetails.Amount,
                Currency = paymentDetails.Currency,
                CardNumberLastFour = cardDetails.CardNumberLastFourDigits,
                ExpiryMonth = cardDetails.ExpiryMonth,
                ExpiryYear = cardDetails.ExpiryYear,
                PaymentId = paymentDetails.Id ?? Guid.NewGuid(),
                PaymentProcessingStatus = PaymentProcessingStatus.Processing,
                Merchant = merchantEntity
            };
            merchantEntity.Payments.Add(paymentEntity);
            _paymentsDbContext.Merchants.Update(merchantEntity);
            await _paymentsDbContext.SaveChangesAsync();
            return paymentEntity.PaymentId;
        }

        public async Task<(CardDetails cardDetails, PaymentDetails paymentDetails, BankPaymentStatus bankPaymentStatus)> GetPaymentById(Guid paymentId, Merchant merchant)
        {
            var payments = await TryGetMerchantPayments(merchant) ?? throw new PaymentNotFoundException($"Payment {paymentId} for merchant {merchant.MerchantId} not found.");
            var payment = payments.ToList().FirstOrDefault(p => p.PaymentId.Equals(paymentId));
            return payment == null
                ? throw new PaymentNotFoundException()
                : (new CardDetails(payment.CardNumberLastFour, payment.ExpiryYear, payment.ExpiryMonth, default),
                new PaymentDetails(payment.PaymentId, payment.Currency, payment.Amount),
                payment.PaymentProcessingStatus.MapToBankPaymentStatus());
        }

        public async Task<(CardDetails cardDetails, PaymentDetails paymentDetails)> UpdatePaymentStatusById(Guid paymentId, BankPaymentStatus bankPaymentStatus, Merchant merchant)
        {
            var payment = await _paymentsDbContext.Payments.FindAsync(paymentId);
            if (payment == null || merchant.MerchantId != payment.Merchant.MerchantId) throw new PaymentNotFoundException($"Payment {paymentId} for merchant {merchant.MerchantId} not found.");
            payment.PaymentProcessingStatus = bankPaymentStatus.MapToPaymentStatus();
            _paymentsDbContext.Payments.Update(payment);
            await _paymentsDbContext.SaveChangesAsync();
            return (new CardDetails(payment.CardNumberLastFour, payment.ExpiryYear, payment.ExpiryMonth, default),
                new PaymentDetails(payment.PaymentId, payment.Currency, payment.Amount));
        }
        private async Task<ICollection<PaymentEntity>> TryGetMerchantPayments(Merchant merchant)
        {
            var merchantEntity = await _paymentsDbContext.Merchants.FindAsync(merchant.MerchantId);
            return merchantEntity == null
                ? throw new MerchantNotFoundException($"Merchant {merchant.MerchantId} not found.")
                : merchantEntity.Payments;
        }
    }
}
