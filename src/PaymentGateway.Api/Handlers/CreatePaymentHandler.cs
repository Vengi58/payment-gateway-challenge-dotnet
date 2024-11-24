
using MediatR;

using PaymentGateway.Api.Commands;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Handlers
{
    public class CreatePaymentHandler : IRequestHandler<CreatePaymentCommand, PostPaymentResponse>
    {
        private readonly ICryptoService _cryptoService;

        public CreatePaymentHandler(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }
        public async Task<PostPaymentResponse> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            var encryptedCardNumber = _cryptoService.Encrypt(request.CardNumber);
            var decryptedCardNumber = _cryptoService.Decrypt(encryptedCardNumber);
            PostPaymentResponse response = new()
            {
                Status = Models.PaymentStatus.Authorized,
                CardNumberLastFour = request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Amount = request.Amount,
                Currency = request.Currency,
                Id = Guid.NewGuid()
            };
            return await Task.FromResult(response);
        }
    }
}
