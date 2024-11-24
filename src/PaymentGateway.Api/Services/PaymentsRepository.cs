
using PaymentGateway.Api.Commands;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class PaymentsRepository : IPaymentRepository
{
    public List<CreatePaymentCommand> Payments = [];

    public void AddPayment(CreatePaymentCommand command)
    {
        Payments.Add(command);
    }
    public CreatePaymentCommand GetPaymentById(Guid paymentId)
    {
        return Payments.FirstOrDefault(p => p.Id == paymentId);
    }
}