
using MediatR;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Domain.Models;
using PaymentGateway.Application.Repository;
using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Application.Payments.Commands.CreatePayment
{
    public class CreatePaymentHandler(IPaymentRepository paymentRepository, IBankSimulator bankSimulator, ICryptoService cryptoService, ILogger<CreatePaymentHandler> logger)
        : IRequestHandler<CreatePaymentCommand, CreatePaymentCommandResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IBankSimulator _bankSimulator = bankSimulator;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly ILogger<CreatePaymentHandler> _logger = logger;

        public async Task<CreatePaymentCommandResponse> Handle(CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handle Create Payment for merchant {createPaymentCommand.Merchant.MerchantId}");
            if (createPaymentCommand.PaymentId != null)
            {
                try
                {
                    _logger.LogInformation($"Payment id \"{createPaymentCommand.PaymentId}\" provided for merchant {createPaymentCommand.Merchant.MerchantId}");
                    var (card, payment, status) = await _paymentRepository.GetPaymentById((Guid)createPaymentCommand.PaymentId, createPaymentCommand.Merchant);

                    _logger.LogInformation($"Payment found for payment id \"{createPaymentCommand.PaymentId}\"  for merchant {createPaymentCommand.Merchant.MerchantId}");
                    _logger.LogInformation($"Handle Create Payment for payment id \"{createPaymentCommand.PaymentId}\"  for merchant {createPaymentCommand.Merchant.MerchantId} completed");
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

            _logger.LogInformation($"Create payment for merchant {createPaymentCommand.Merchant.MerchantId}");
            Guid paymentId = await _paymentRepository.CreatePayment(cardDetails, paymentDetails, createPaymentCommand.Merchant);

            _logger.LogInformation($"Payment with payment id { paymentId} for merchant {createPaymentCommand.Merchant.MerchantId} created");
            _logger.LogInformation($"Payment with payment id {paymentId} for merchant {createPaymentCommand.Merchant.MerchantId} sent to bank");
            var bankPaymentStatusResult = await _bankSimulator.PostPayment(createPaymentCommand.MapToBankCardDetails(), paymentDetails);

            _logger.LogInformation($"Bank response for payment with payment id {paymentId} : {bankPaymentStatusResult}");
            (cardDetails, paymentDetails) = await _paymentRepository.UpdatePaymentStatusById(paymentId, bankPaymentStatusResult, createPaymentCommand.Merchant);
            _logger.LogInformation($"Status of payment with payment id {paymentId} for merchant {createPaymentCommand.Merchant.MerchantId} updated");

            _logger.LogInformation($"Handle Create Payment for payment id {paymentId} for merchant {createPaymentCommand.Merchant.MerchantId} completed");
            return (paymentId, bankPaymentStatusResult, cardDetails, paymentDetails).MapToCreateCreatePaymentResponse(_cryptoService.Decrypt);
        }

        private CreatePaymentCommand UpdateCardNumberToLastFourDigits(CreatePaymentCommand createPaymentCommand) => createPaymentCommand with { CardNumber = createPaymentCommand.CardNumber[^4..] };
    }
}
