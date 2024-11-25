
using MediatR;
using PaymentGateway.Api.Services.BankSimulator;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Repository;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentHandler(IPaymentRepository paymentRepository, IBankSimulator bankSimulator)
        : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IBankSimulator _bankSimulator = bankSimulator;

        public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken)
        {
            CardDetails cardDetails = createPaymentCommand.MapToCardDetails();
            PaymentDetails paymentDetails = createPaymentCommand.MapToPaymentDetails();

            Guid paymentId = await _paymentRepository.CreatePayment(cardDetails, paymentDetails);

            var bankPaymentStatusResult = await _bankSimulator.PostPayment(cardDetails, paymentDetails with { Id = paymentId});

            (cardDetails, paymentDetails) = await _paymentRepository.UpdatePaymentStatusById(paymentId, bankPaymentStatusResult);

            return (paymentId, bankPaymentStatusResult, cardDetails, paymentDetails).MapToCreateCreatePaymentResponse() with { CardNumberLastFourDigits = cardDetails.CardNumber[^4..] };
        }
    }
}
