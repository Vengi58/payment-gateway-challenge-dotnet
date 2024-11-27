
using MediatR;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Domain.Models;
using PaymentGateway.Application.Repository;
using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Exceptions;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentHandler(IPaymentRepository paymentRepository, IBankSimulator bankSimulator, ICryptoService cryptoService)
        : IRequestHandler<CreatePaymentCommand, CreatePaymentCommandResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IBankSimulator _bankSimulator = bankSimulator;
        private readonly ICryptoService _cryptoService = cryptoService;

        public async Task<CreatePaymentCommandResponse> Handle(CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken)
        {
            if (createPaymentCommand.PaymentId != null)
            {
                try
                {
                    var (card, payment, status) = await _paymentRepository.GetPaymentById((Guid)createPaymentCommand.PaymentId, createPaymentCommand.Merchant);
                    return ((Guid)createPaymentCommand.PaymentId, status, card, payment).MapToCreateCreatePaymentResponse(_cryptoService.Decrypt);
                }
                catch (Exception e)
                {
                    if (e is not PaymentNotFoundException)
                    {
                        throw;
                    }
                }
            }

            CardDetails cardDetails = UpdateCardNumberToLastFourDigits(createPaymentCommand).MapToCardDetails(_cryptoService.Encrypt);
            PaymentDetails paymentDetails = createPaymentCommand.MapToPaymentDetails();

            Guid paymentId = await _paymentRepository.CreatePayment(cardDetails, paymentDetails, createPaymentCommand.Merchant);

            var bankPaymentStatusResult = await _bankSimulator.PostPayment(createPaymentCommand.MapToBankCardDetails(), paymentDetails);

            (cardDetails, paymentDetails) = await _paymentRepository.UpdatePaymentStatusById(paymentId, bankPaymentStatusResult, createPaymentCommand.Merchant);

            return (paymentId, bankPaymentStatusResult, cardDetails, paymentDetails).MapToCreateCreatePaymentResponse(_cryptoService.Decrypt);
        }

        private CreatePaymentCommand UpdateCardNumberToLastFourDigits(CreatePaymentCommand createPaymentCommand) => createPaymentCommand with { CardNumber = createPaymentCommand.CardNumber[^4..] };
    }
}
