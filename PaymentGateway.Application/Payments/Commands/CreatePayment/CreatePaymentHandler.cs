
using MediatR;

using PaymentGateway.Api.Commands;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Services.BankSimulator;

namespace PaymentGateway.Api.Handlers
{
    public class CreatePaymentHandler(IPaymentRepository paymentRepository, IBankSimulator bankSimulator, ICryptoService cryptoService) 
        : IRequestHandler<CreatePaymentCommand, PostPaymentResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly IBankSimulator _bankSimulator = bankSimulator;
        private readonly ICryptoService _cryptoService = cryptoService;

        public async Task<PostPaymentResponse> Handle(CreatePaymentCommand createPaymentCommand, CancellationToken cancellationToken)
        {
            //var encryptedCardNumber = _cryptoService.Encrypt(createPaymentCommand.CardNumber);
            //var decryptedCardNumber = _cryptoService.Decrypt(encryptedCardNumber);

            PostPaymentResponse response;

            var payment = _paymentRepository.GetPaymentById(createPaymentCommand.Id);

            if (payment != null)
            {
                response = new()
                {
                    Status = Models.PaymentStatus.Authorized,
                    CardNumberLastFour = payment.CardNumber,
                    ExpiryMonth = payment.ExpiryMonth,
                    ExpiryYear = payment.ExpiryYear,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Id = payment.Id
                };
                return await Task.FromResult(response);
            }
            

            _paymentRepository.AddPayment(createPaymentCommand);

            var padding = createPaymentCommand.ExpiryMonth < 10 ? "0" : string.Empty;
            var bankPaymentResult = await _bankSimulator.PostPayment(new PostBankPaymentRequest(
                createPaymentCommand.CardNumber,
                $"{padding}{createPaymentCommand.ExpiryMonth}/{createPaymentCommand.ExpiryYear}", 
                createPaymentCommand.Currency,
                createPaymentCommand.Amount,  
                createPaymentCommand.Cvv.ToString()));
            
            response = new()
            {
                Status = bankPaymentResult.authorized ? PaymentStatus.Authorized : PaymentStatus.Declined,
                CardNumberLastFour = createPaymentCommand.CardNumber,
                ExpiryMonth = createPaymentCommand.ExpiryMonth,
                ExpiryYear = createPaymentCommand.ExpiryYear,
                Amount = createPaymentCommand.Amount,
                Currency = createPaymentCommand.Currency,
                Id = createPaymentCommand.Id
            };
            return await Task.FromResult(response);
        }
    }
}
