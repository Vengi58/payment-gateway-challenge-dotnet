
using MediatR;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Domain.Models;
using PaymentGateway.Application.Repository;
using PaymentGateway.Application.Encryption;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentHandler(IPaymentRepository paymentRepository, IBankSimulator bankSimulator, ICryptoService cryptoService)
        : IRequestHandler<CreatePaymentCommand, CreatePaymentResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IBankSimulator _bankSimulator = bankSimulator;
        private readonly ICryptoService _cryptoService = cryptoService;

        public async Task<CreatePaymentResponse> Handle(CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken)
        {
            if (createPaymentCommand.Id != null)
            {
                var (card, payment, status) = await _paymentRepository.GetPaymentById(createPaymentCommand.Id);
                if (card != null && payment != null)
                {
                    return ((Guid)createPaymentCommand.Id, status, card, payment).MapToCreateCreatePaymentResponse(_cryptoService.Decrypt);
                }
            }

            CardDetails cardDetails = UpdateCardNumberToLastFourDigits(createPaymentCommand).MapToCardDetails(_cryptoService.Encrypt);
            PaymentDetails paymentDetails = createPaymentCommand.MapToPaymentDetails();

            Guid paymentId = await _paymentRepository.CreatePayment(cardDetails, paymentDetails);

            var bankPaymentStatusResult = await _bankSimulator.PostPayment(createPaymentCommand.MapToCardDetailsFull(), paymentDetails);

            (cardDetails, paymentDetails) = await _paymentRepository.UpdatePaymentStatusById(paymentId, bankPaymentStatusResult);

            return (paymentId, bankPaymentStatusResult, cardDetails, paymentDetails).MapToCreateCreatePaymentResponse(_cryptoService.Decrypt);
        }

        private CreatePaymentCommand UpdateCardNumberToLastFourDigits(CreatePaymentCommand createPaymentCommand) => createPaymentCommand with { CardNumber = createPaymentCommand.CardNumber[^4..] };
    }
}
