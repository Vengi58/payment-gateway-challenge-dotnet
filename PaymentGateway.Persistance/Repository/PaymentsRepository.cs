using PaymentGateway.Application.Repository;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;
using PaymentGateway.Persistance.Mappings;

namespace PaymentGateway.Persistance.Repository;

public class PaymentsRepository() : IPaymentRepository
{
    internal List<Payment> Payments = [];

    public async Task<Guid> CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails)
    {
        Payment payment = (cardDetails, paymentDetails, PaymentProcessingStatus.Processing).MapToPayment();

        Payments.Add(payment);
        return await Task.FromResult(payment.Id);
    }

    public async Task<(CardDetails cardDetails, PaymentDetails paymentDetails, BankPaymentStatus bankPaymentStatus)> GetPaymentById(Guid? paymentId)
    {
        var payment = Payments.FirstOrDefault(p => p.Id.Equals(paymentId));

        if (payment == null) return (null, null, BankPaymentStatus.Rejected);

        return new(payment.MapToCardDetails(), payment.MapToPaymentDetails(), payment.PaymentStatus.MapToBankPaymentStatus());
    }

    public async Task<(CardDetails? cardDetails, PaymentDetails paymentDetails)> UpdatePaymentStatusById(Guid? paymentId, BankPaymentStatus bankPaymentStatus)
    {
        var paymentIndex = Payments.FindIndex(p => p.Id.Equals(paymentId));
        if (paymentIndex == -1) return (null, null);
        var payment = Payments[paymentIndex];
        Payments[paymentIndex] = Payments[paymentIndex] with { PaymentStatus = bankPaymentStatus.MapToPaymentStatus() };
        return await Task.FromResult((Payments[paymentIndex].MapToCardDetails(), Payments[paymentIndex].MapToPaymentDetails()));

    }
}