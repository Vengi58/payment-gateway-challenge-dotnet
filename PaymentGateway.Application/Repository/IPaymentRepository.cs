using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Repository;

public interface IPaymentRepository
{
    public Task<Guid> CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails, Merchant merchant);
    public Task<(CardDetails cardDetails, PaymentDetails paymentDetails, BankPaymentStatus bankPaymentStatus)> GetPaymentById(Guid paymentId, Merchant merchant);
    public Task<(CardDetails cardDetails, PaymentDetails paymentDetails)> UpdatePaymentStatusById(Guid paymentId, BankPaymentStatus bankPaymentStatus, Merchant merchant);
}
