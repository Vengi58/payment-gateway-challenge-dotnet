using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Entities;
using PaymentGateway.Services.Encryption;

namespace PaymentGateway.Persistance.Repository;

public class PaymentsRepository : IPaymentRepository
{
    private readonly ICryptoService _cryptoService;
    public List<Payment> Payments = [];

    public PaymentsRepository(ICryptoService cryptoService)
    {
        _cryptoService = cryptoService;
    }

    public Guid CreatePayment(CardDetails cardDetails, PaymentDetails paymentDetails)
    {
        Payment payment = new Payment()
        {
            CardNumber = _cryptoService.Encrypt(cardDetails.CardNumber),
            Cvv = _cryptoService.Encrypt(cardDetails.Cvv.ToString()),
            ExpiryMonth = cardDetails.ExpiryMonth,
            ExpiryYear = cardDetails.ExpiryYear,
            Amount = paymentDetails.Amount,
            Id = paymentDetails.Id ?? Guid.NewGuid()
        };
        Payments.Add(payment);
        return payment.Id;
    }

    Tuple<CardDetails, PaymentDetails> IPaymentRepository.GetPaymentById(Guid? paymentId)
    {
        if (paymentId == null)
        {
            return new(null, null);
        }

        var payment = Payments.FirstOrDefault(p => p.Id.Equals(paymentId));
        if (payment == null)
        {
            return new(null, null);
        }

        CardDetails cardDetails = new CardDetails()
        {
            CardNumber = _cryptoService.Decrypt(payment.CardNumber),
            Cvv = Convert.ToInt32(_cryptoService.Decrypt(payment.Cvv)),
            ExpiryMonth = payment.ExpiryMonth,
            ExpiryYear = payment.ExpiryYear
        };
        PaymentDetails paymentDetails = new PaymentDetails()
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = payment.Currency
        };
        return new(cardDetails, paymentDetails);
    }
}