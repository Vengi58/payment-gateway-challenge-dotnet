using MediatR;

using Microsoft.Extensions.Logging;

using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Application.Repository;

namespace PaymentGateway.Application.Payments.Queries.GetPayment
{
    public class GetPaymentQueryHandler(IPaymentRepository paymentRepository, ICryptoService cryptoService, ILogger<GetPaymentQueryHandler> logger) : IRequestHandler<GetPaymentQuery, GetPaymentQueryResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly ILogger<GetPaymentQueryHandler> _logger = logger;

        public async Task<GetPaymentQueryResponse> Handle(GetPaymentQuery getPaymentQuery, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Handle Get Payment for payment id {getPaymentQuery.PaymentId} for merchant {getPaymentQuery.Merchant.MerchantId}");
            var (cardDetails, paymentDetails, bankPaymentStatus) = await _paymentRepository.GetPaymentById(getPaymentQuery.PaymentId, getPaymentQuery.Merchant);

            _logger.LogInformation($"Handle Get Payment for payment id {getPaymentQuery.PaymentId} for merchant {getPaymentQuery.Merchant.MerchantId} completed");
            return await Task.FromResult((getPaymentQuery.PaymentId, bankPaymentStatus, cardDetails, paymentDetails).MapToCreateGetPaymentResponse(_cryptoService.Decrypt));
        }
    }
}
