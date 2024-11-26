using Moq;

using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Payments.Queries.GetPayment;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Application.Repository;
using PaymentGateway.Services.Encryption;

namespace PaymentGateway.Application.Tests
{
    public class GetPaymentQueryHandlerTests
    {
        readonly GetPaymentQueryHandler _getPaymentQueryHandler;
        private readonly ICryptoService _cryptoService;
        private readonly Mock<IBankSimulator> _bankSimulatorMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;

        private readonly CardDetails cardDetails;
        private readonly Guid paymentId;
        private readonly PaymentDetails paymentDetails;
        public GetPaymentQueryHandlerTests()
        {
            _bankSimulatorMock = new Mock<IBankSimulator>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _cryptoService = new RsaCryptoService();
            _getPaymentQueryHandler = new GetPaymentQueryHandler(_paymentRepositoryMock.Object, _cryptoService);

            cardDetails = new(_cryptoService.Encrypt("2222405343248877"), 2025, 4, _cryptoService.Encrypt("123"));
            paymentId = Guid.NewGuid();
            paymentDetails = new(paymentId, "GBP", 100);
        }

        [Fact]
        public async Task GetPaymentHandler_ValidPatmentID_ReturnsPaymentDetailsSuccessfully()
        {
            //Arrange
            _paymentRepositoryMock
                .Setup(_ => _.GetPaymentById(paymentId))
                .ReturnsAsync((cardDetails, paymentDetails, BankPaymentStatus.Authorized));

            //Act
            var getPaymentResponse = await _getPaymentQueryHandler.Handle(new(paymentId), new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(paymentId, It.IsAny<BankPaymentStatus>()), Times.Never);
            _paymentRepositoryMock.Verify(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>()), Times.Never);
        }

        [Fact]
        public async Task GetPaymentHandler_InvalidPatmentID_ThrowsPaymentNotFoundException()
        {
            //Arrange
            Guid paymentId = Guid.NewGuid();

            _paymentRepositoryMock
                .Setup(_ => _.GetPaymentById(paymentId))
                .ReturnsAsync((It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), BankPaymentStatus.Rejected));

            //Act
            //Assert
            var exc = await Assert.ThrowsAsync<PaymentNotFoundException>(async () => await _getPaymentQueryHandler.Handle(new(paymentId), new CancellationToken()));
            Assert.Equal($"Payment with payment id {paymentId} not found.", exc.Message);
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(paymentId, It.IsAny<BankPaymentStatus>()), Times.Never);
            _paymentRepositoryMock.Verify(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>()), Times.Never);
        }
    }
}
