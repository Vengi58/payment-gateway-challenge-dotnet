using PaymentGateway.Domain.Models;

namespace PaymentGateway.Persistance.Repository;

public interface IPaymentRepository
{
    public Guid CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails);
    public Tuple<CardDetails, PaymentDetails> GetPaymentById(Guid? paymentId);
}
