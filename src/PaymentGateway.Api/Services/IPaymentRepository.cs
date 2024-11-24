using PaymentGateway.Api.Commands;

namespace PaymentGateway.Api.Services
{
    public interface IPaymentRepository
    {
        public void AddPayment(CreatePaymentCommand command);
        public CreatePaymentCommand GetPaymentById(Guid paymentId);
    }
}
