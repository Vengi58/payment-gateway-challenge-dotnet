using Moq;

using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Payments.Queries.GetPayment;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Application.Repository;
using PaymentGateway.Services.Encryption;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Application.Tests
{
    public class GetPaymentQueryHandlerTests
    {
        readonly GetPaymentQueryHandler _getPaymentQueryHandler;
        private readonly ICryptoService _cryptoService;
        private readonly Mock<IBankSimulator> _bankSimulatorMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<ILogger<GetPaymentQueryHandler>> _loggerMock;

        private readonly CardDetails _cardDetails;
        private readonly Guid _paymentId;
        private readonly PaymentDetails _paymentDetails;
        private readonly Merchant _merchant;
        public GetPaymentQueryHandlerTests()
        {
            _bankSimulatorMock = new Mock<IBankSimulator>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _cryptoService = new RsaCryptoService();
            _loggerMock = new Mock<ILogger<GetPaymentQueryHandler>>();
            _getPaymentQueryHandler = new GetPaymentQueryHandler(_paymentRepositoryMock.Object, _cryptoService, _loggerMock.Object);

            _cardDetails = new(_cryptoService.Encrypt("2222405343248877"), 2025, 4, _cryptoService.Encrypt("123"));
            _paymentId = Guid.NewGuid();
            _paymentDetails = new(_paymentId, "GBP", 100);
            _merchant = new(Guid.Parse("47f729c1-c863-4403-ae2e-6e836bf44fee"));
        }

        [Fact]
        public async Task GetPaymentHandler_ValidPatmentID_ReturnsPaymentDetailsSuccessfully()
        {
            //Arrange
            _paymentRepositoryMock
                .Setup(_ => _.GetPaymentById(_paymentId, _merchant))
                .ReturnsAsync((_cardDetails, _paymentDetails, BankPaymentStatus.Authorized));

            //Act
            var getPaymentResponse = await _getPaymentQueryHandler.Handle(new(_paymentId, _merchant), new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(_paymentId, It.IsAny<BankPaymentStatus>(), _merchant), Times.Never);
            _paymentRepositoryMock.Verify(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), _merchant), Times.Never);
        }

        [Fact]
        public async Task GetPaymentHandler_InvalidPatmentID_ThrowsPaymentNotFoundException()
        {
            //Arrange
            Guid paymentId = Guid.NewGuid();

            _paymentRepositoryMock
                .Setup(_ => _.GetPaymentById(paymentId, _merchant))
                .ThrowsAsync(new PaymentNotFoundException($"Payment with payment id {paymentId} not found."));

            //Act
            //Assert
            var exc = await Assert.ThrowsAsync<PaymentNotFoundException>(async () => await _getPaymentQueryHandler.Handle(new(paymentId, _merchant), new CancellationToken()));
            Assert.Equal($"Payment with payment id {paymentId} not found.", exc.Message);
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(paymentId, It.IsAny<BankPaymentStatus>(), _merchant), Times.Never);
            _paymentRepositoryMock.Verify(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), _merchant), Times.Never);
        }
    }
}
