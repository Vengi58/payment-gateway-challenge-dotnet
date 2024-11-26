using Moq;

using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Payments.Commands.CreatePayment;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Application.Repository;
using PaymentGateway.Services.Encryption;

namespace PaymentGateway.Application.Tests
{
    public class CreatePaymentHandlerTests
    {
        readonly CreatePaymentHandler _createPaymentHandler;
        private readonly ICryptoService _cryptoService;
        private readonly Mock<IBankSimulator> _bankSimulatorMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;

        private readonly BankCardDetails cardDetailsFull;
        private readonly CardDetails cardDetails;
        private readonly Guid paymentId;
        private readonly PaymentDetails paymentDetails;
        public CreatePaymentHandlerTests()
        {
            _cryptoService = new RsaCryptoService();
            _bankSimulatorMock = new Mock<IBankSimulator>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _createPaymentHandler = new CreatePaymentHandler(_paymentRepositoryMock.Object, _bankSimulatorMock.Object, _cryptoService);
            cardDetailsFull = new("2222405343248877", 2025, 4, "123");
            cardDetails = new(_cryptoService.Encrypt(cardDetailsFull.CardNumber), cardDetailsFull.ExpiryYear, cardDetailsFull.ExpiryMonth, _cryptoService.Encrypt(cardDetailsFull.Cvv));
            paymentId = Guid.NewGuid();
            paymentDetails = new(paymentId, "GBP", 100);
        }

        [Fact]
        public async Task CreatePaymentHandler_AuthorizedPayment_PersistedWitCompleted()
        {
            //Arrange
            //CardDetails cardDetails = new(_cryptoService.Encrypt("2222405343248877"), 2025,4, _cryptoService.Encrypt("123"));
            //PaymentDetails paymentDetails = new(paymentId, "GBP", 100);

            _bankSimulatorMock
                .Setup(_ => _.PostPayment(cardDetailsFull, paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Authorized);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>()))
                .ReturnsAsync(paymentId);
            _paymentRepositoryMock
                .Setup(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Authorized))
                .ReturnsAsync((cardDetails, paymentDetails));

            //Act
            var createPaymentResponse = await _createPaymentHandler.Handle(new(
                paymentId,
                paymentDetails.Currency,
                paymentDetails.Amount,
                _cryptoService.Decrypt(cardDetails.CardNumberLastFourDigits),
                cardDetails.ExpiryMonth,
                cardDetails.ExpiryYear,
                Convert.ToInt32(_cryptoService.Decrypt(cardDetails.Cvv))),
                new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Authorized), Times.Once);
        }


        [Fact]
        public async Task CreatePaymentHandler_DeclinedPayment_PersistedWithDeclined()
        {
            //Arrange

            _bankSimulatorMock
                .Setup(_ => _.PostPayment(cardDetailsFull, paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Declined);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>()))
                .ReturnsAsync(paymentId);
            _paymentRepositoryMock
                .Setup(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Declined))
                .ReturnsAsync((cardDetails, paymentDetails));

            //Act
            var createPaymentResponse = await _createPaymentHandler.Handle(new(
                paymentId,
                paymentDetails.Currency,
                paymentDetails.Amount,
                _cryptoService.Decrypt(cardDetails.CardNumberLastFourDigits),
                cardDetails.ExpiryMonth,
                cardDetails.ExpiryYear,
                Convert.ToInt32(_cryptoService.Decrypt(cardDetails.Cvv))),
                new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Declined), Times.Once);
        }
    }
}