using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Api.Services.BankSimulator
{
    public interface IBankSimulator
    {
        public Task<BankPaymentStatus> PostPayment(CardDetails cardDetails, PaymentDetails paymentDetails); 
    }
}
