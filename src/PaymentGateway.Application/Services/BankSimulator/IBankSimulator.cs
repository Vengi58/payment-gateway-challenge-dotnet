using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Application.Services.BankSimulator
{
    public interface IBankSimulator
    {
        public Task<BankPaymentStatus> PostPayment(BankCardDetails cardDetails, PaymentDetails paymentDetails); 
    }
}
