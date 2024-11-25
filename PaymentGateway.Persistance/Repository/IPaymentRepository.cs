using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;

namespace PaymentGateway.Persistance.Repository;

public interface IPaymentRepository
{
    public Task<Guid> CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails);
    public Task<(CardDetails cardDetails, PaymentDetails paymentDetails, BankPaymentStatus bankPaymentStatus)> GetPaymentById(Guid? paymentId);
    public Task<(CardDetails cardDetails, PaymentDetails paymentDetails)> UpdatePaymentStatusById(Guid? paymentId, BankPaymentStatus bankPaymentStatus);
}
