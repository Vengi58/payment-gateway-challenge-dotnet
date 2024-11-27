using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Repository;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;
using PaymentGateway.Persistance.Mappings;

namespace PaymentGateway.Persistance.Repository;

public class PaymentsRepository : IPaymentRepository
{
    internal Dictionary<Guid, List<Payment>> MerchantPayments = [];
    public PaymentsRepository()
    {
        AddMerchants();
    }

    public async Task<Guid> CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails, Merchant merchant)
    {
        Payment payment = (cardDetails, paymentDetails, PaymentProcessingStatus.Processing).MapToPayment();
        if (!MerchantPayments.TryGetValue(merchant.MerchantId, out List<Payment>? merchantPayments))
        {
            merchantPayments = ([]);
            MerchantPayments.Add(merchant.MerchantId, merchantPayments);
        }

        merchantPayments.Add(payment);
        return await Task.FromResult(payment.Id);
    }

    public async Task<(CardDetails cardDetails, PaymentDetails paymentDetails, BankPaymentStatus bankPaymentStatus)> GetPaymentById(Guid paymentId, Merchant merchant)
    {
        var payment = TryGetMerchantPayments(merchant)!.FirstOrDefault(p => p.Id.Equals(paymentId));

        return payment == null
            ? throw new PaymentNotFoundException($"Payment {paymentId} for merchant {merchant.MerchantId} not found.")
            : new(payment.MapToCardDetails(), payment.MapToPaymentDetails(), payment.PaymentStatus.MapToBankPaymentStatus());
    }

    public async Task<(CardDetails? cardDetails, PaymentDetails paymentDetails)> UpdatePaymentStatusById(Guid paymentId, BankPaymentStatus bankPaymentStatus, Merchant merchant)
    {
        var payment = TryGetMerchantPayments(merchant)!.FirstOrDefault(p => p.Id.Equals(paymentId)) ?? throw new PaymentNotFoundException($"Payment {paymentId} for merchant {merchant.MerchantId} not found.");
        payment = payment with { PaymentStatus = bankPaymentStatus.MapToPaymentStatus() };
        return await Task.FromResult((payment.MapToCardDetails(), payment.MapToPaymentDetails()));
    }

    private List<Payment>? TryGetMerchantPayments(Merchant merchant)
    {
        return !MerchantPayments.TryGetValue(merchant.MerchantId, out List<Payment>? merchantPayments)
            ? throw new MerchantNotFoundException($"Merchant {merchant.MerchantId} not found.")
            : merchantPayments;
    }
    private void AddMerchants()
    {
        MerchantPayments.Add(Guid.Parse("47f729c1-c863-4403-ae2e-6e836bf44fee"), []);
        MerchantPayments.Add(Guid.Parse("3e1ae11d-4652-468b-9c4c-afd701c9d642"), []);
    }
}