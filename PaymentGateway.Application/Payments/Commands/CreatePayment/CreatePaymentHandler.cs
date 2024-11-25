
using MediatR;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services.BankSimulator;
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
            CreatePaymentResponse response;

            if (createPaymentCommand.Id != null)
            {
                var payment = _paymentRepository.GetPaymentById(createPaymentCommand.Id);

                if (payment.Item1 != null && payment.Item2 != null)
                {
                    response = new()
                    {
                        Status = PaymentStatus.Authorized,
                        PaymentId = (Guid)createPaymentCommand.Id
                    };
                    return await Task.FromResult(response);
                }
            }
            

            Guid paymentId = _paymentRepository.CreatePayment(
                new CardDetails() { 
                    CardNumber = createPaymentCommand.CardNumber,
                    Cvv = createPaymentCommand.Cvv,
                    ExpiryMonth = createPaymentCommand.ExpiryMonth,
                    ExpiryYear = createPaymentCommand.ExpiryYear
                },
                new PaymentDetails()
                {
                    Amount = createPaymentCommand.Amount,
                    Currency = createPaymentCommand.Currency,
                    Id = createPaymentCommand.Id
                });

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
                PaymentId = paymentId
            };
            return await Task.FromResult(response);
        }
    }
}
