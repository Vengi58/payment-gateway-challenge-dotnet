﻿using MediatR;

using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Mappings;
using PaymentGateway.Application.Repository;

namespace PaymentGateway.Application.Payments.Queries.GetPayment
{
    public class GetPaymentQueryHandler(IPaymentRepository paymentRepository, ICryptoService cryptoService) : IRequestHandler<GetPaymentQuery, GetPaymentResponse>
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;
        private readonly ICryptoService _cryptoService = cryptoService;


        public async Task<GetPaymentResponse> Handle(GetPaymentQuery getPaymentQuery, CancellationToken cancellationToken)
        {
            var (cardDetails, paymentDetails, bankPaymentStatus) = await _paymentRepository.GetPaymentById(getPaymentQuery.PaymentId);
            return paymentDetails == null || cardDetails == null
                ? throw new PaymentNotFoundException($"Payment with payment id {getPaymentQuery.PaymentId} not found.")
                : await Task.FromResult((getPaymentQuery.PaymentId, bankPaymentStatus, cardDetails, paymentDetails).MapToCreateGetPaymentResponse(_cryptoService.Decrypt));
        }
    }
}
