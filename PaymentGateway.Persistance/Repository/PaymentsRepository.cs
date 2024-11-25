using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;
using PaymentGateway.Persistance.Mappings;
using PaymentGateway.Services.Encryption;

namespace PaymentGateway.Persistance.Repository;

public class PaymentsRepository(ICryptoService cryptoService) : IPaymentRepository
{
    private readonly ICryptoService _cryptoService = cryptoService;
    internal List<Payment> Payments = [];

    public async Task<Guid> CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails)
    {
        (var p, var c, var s) = await GetPaymentById(paymentDetails.Id);
        if (p != null && c != null)
        {
            return await Task.FromResult((Guid)paymentDetails.Id); //might be better to throw an exception and notify user that the payment already exists
        }
        Payment payment = (cardDetails, paymentDetails, PaymentStatus.Created).MapToPayment(_cryptoService.Encrypt);

        Payments.Add(payment);
        return await Task.FromResult(payment.Id);
    }

    public async Task<(CardDetails cardDetails, PaymentDetails paymentDetails, BankPaymentStatus bankPaymentStatus)> GetPaymentById(Guid? paymentId)
    {
        var payment = Payments.FirstOrDefault(p => p.Id.Equals(paymentId));

        if (payment == null) return (null, null, BankPaymentStatus.Rejected);

        return new(payment.MapToCardDetails(_cryptoService.Decrypt), payment.MapToPaymentDetails(), payment.PaymentStatus.MapToBankPaymentStatus());
    }

    public async Task<(CardDetails? cardDetails, PaymentDetails paymentDetails)> UpdatePaymentStatusById(Guid? paymentId, BankPaymentStatus bankPaymentStatus)
    {
        var paymentIndex = Payments.FindIndex(p => p.Id.Equals(paymentId));
        if (paymentIndex == -1) return (null, null);
        var payment = Payments[paymentIndex];
        Payments[paymentIndex] = Payments[paymentIndex] with { PaymentStatus = bankPaymentStatus.MapToPaymentStatus() };
        return await Task.FromResult((Payments[paymentIndex].MapToCardDetails(_cryptoService.Decrypt), Payments[paymentIndex].MapToPaymentDetails()));

    }
}