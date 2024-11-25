using Moq;

using PaymentGateway.Api.Services.BankSimulator;
using PaymentGateway.Application.Payments.Commands.CreatePayment;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Repository;
using PaymentGateway.Services.Encryption;

namespace PaymentGateway.Application.Tests
{
    public class CreatePaymentHandlerTests
    {
        readonly CreatePaymentHandler _createPaymentHandler;
        private readonly Mock<IBankSimulator> _bankSimulatorMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        public CreatePaymentHandlerTests()
        {
            _bankSimulatorMock = new Mock<IBankSimulator>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _createPaymentHandler = new CreatePaymentHandler(_paymentRepositoryMock.Object, _bankSimulatorMock.Object);
        }

        [Fact]
        public async Task CreatePaymentHandler_AuthorizedPayment_PersistedWitCompleted()
        {
            //Arrange
            Guid paymentId = Guid.NewGuid();
            CardDetails cardDetails = new("2222405343248877", 2025,4, 123);
            PaymentDetails paymentDetails = new(paymentId, "GBP", 100);

            _bankSimulatorMock
                .Setup(_ => _.PostPayment(cardDetails, paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Authorized);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(cardDetails, paymentDetails))
                .ReturnsAsync(paymentId);
            _paymentRepositoryMock
                .Setup(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Authorized))
                .ReturnsAsync((cardDetails, paymentDetails));

            //Act
            var createPaymentResponse = await _createPaymentHandler.Handle(new(
                paymentId,
                paymentDetails.Currency,
                paymentDetails.Amount,
                cardDetails.CardNumber,
                cardDetails.ExpiryMonth,
                cardDetails.ExpiryYear,
                cardDetails.Cvv),
                new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Authorized), Times.Once);
        }


        [Fact]
        public async Task CreatePaymentHandler_DeclinedPayment_PersistedWithDeclined()
        {
            //Arrange
            Guid paymentId = Guid.NewGuid();
            CardDetails cardDetails = new("2222405343248877", 2025, 4, 123);
            PaymentDetails paymentDetails = new(paymentId, "GBP", 100);

            _bankSimulatorMock
                .Setup(_ => _.PostPayment(cardDetails, paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Declined);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(cardDetails, paymentDetails))
                .ReturnsAsync(paymentId);
            _paymentRepositoryMock
                .Setup(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Declined))
                .ReturnsAsync((cardDetails, paymentDetails));

            //Act
            var createPaymentResponse = await _createPaymentHandler.Handle(new(
                paymentId,
                paymentDetails.Currency,
                paymentDetails.Amount,
                cardDetails.CardNumber,
                cardDetails.ExpiryMonth,
                cardDetails.ExpiryYear,
                cardDetails.Cvv),
                new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(paymentId, BankPaymentStatus.Declined), Times.Once);
        }
    }
}