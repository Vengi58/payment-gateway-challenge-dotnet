using Moq;

using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Payments.Commands.CreatePayment;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Application.Repository;
using PaymentGateway.Services.Encryption;
using PaymentGateway.Application.Exceptions;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Application.Tests
{
    public class CreatePaymentHandlerTests
    {
        readonly CreatePaymentHandler _createPaymentHandler;
        private readonly ICryptoService _cryptoService;
        private readonly Mock<IBankSimulator> _bankSimulatorMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<ILogger<CreatePaymentHandler>> _loggerMock;

        private readonly BankCardDetails _bankCardDetails;
        private readonly CardDetails _cardDetails;
        private readonly Guid _paymentId;
        private readonly PaymentDetails _paymentDetails;
        private readonly Merchant _merchant;
        public CreatePaymentHandlerTests()
        {
            _cryptoService = new RsaCryptoService();
            _bankSimulatorMock = new Mock<IBankSimulator>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _loggerMock = new Mock<ILogger<CreatePaymentHandler>>();
            _createPaymentHandler = new CreatePaymentHandler(_paymentRepositoryMock.Object, _bankSimulatorMock.Object, _cryptoService, _loggerMock.Object);

            _bankCardDetails = new("2222405343248877", 2025, 4, "123");
            _cardDetails = new(_cryptoService.Encrypt(_bankCardDetails.CardNumber), _bankCardDetails.ExpiryYear, _bankCardDetails.ExpiryMonth, _cryptoService.Encrypt(_bankCardDetails.Cvv));
            _paymentId = Guid.NewGuid();
            _paymentDetails = new(_paymentId, "GBP", 100);
            _merchant = new(Guid.Parse("47f729c1-c863-4403-ae2e-6e836bf44fee"));
        }

        [Fact]
        public async Task CreatePaymentHandler_AuthorizedPayment_PersistedWithCompleted()
        {
            //Arrange
            _bankSimulatorMock
                .Setup(_ => _.PostPayment(_bankCardDetails, _paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Authorized);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), It.IsAny<Merchant>()))
                .ReturnsAsync(_paymentId);
            _paymentRepositoryMock
                .Setup(_ => _.GetPaymentById(_paymentId, _merchant))
                .ThrowsAsync(new PaymentNotFoundException());
            _paymentRepositoryMock
                .Setup(_ => _.UpdatePaymentStatusById(_paymentId, BankPaymentStatus.Authorized, _merchant))
                .ReturnsAsync((_cardDetails, _paymentDetails));

            //Act
            var createPaymentResponse = await _createPaymentHandler.Handle(new(
                _paymentId,
                _paymentDetails.Currency,
                _paymentDetails.Amount,
                _cryptoService.Decrypt(_cardDetails.CardNumberLastFourDigits),
                _cardDetails.ExpiryMonth,
                _cardDetails.ExpiryYear,
                Convert.ToInt32(_cryptoService.Decrypt(_cardDetails.Cvv)),
                _merchant),
                new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(_paymentId, BankPaymentStatus.Authorized, _merchant), Times.Once);
        }

        [Fact]
        public async Task CreatePaymentHandler_DeclinedPayment_ReturnsDeclined()
        {
            //Arrange

            _bankSimulatorMock
                .Setup(_ => _.PostPayment(_bankCardDetails, _paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Declined);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), It.IsAny<Merchant>()))
                .ReturnsAsync(_paymentId);
            _paymentRepositoryMock
                .Setup(_ => _.GetPaymentById(_paymentId, _merchant))
                .ThrowsAsync(new PaymentNotFoundException());
            _paymentRepositoryMock
                .Setup(_ => _.UpdatePaymentStatusById(_paymentId, BankPaymentStatus.Declined, _merchant))
                .ReturnsAsync((_cardDetails, _paymentDetails));

            //Act
            var createPaymentResponse = await _createPaymentHandler.Handle(new(
                _paymentId,
                _paymentDetails.Currency,
                _paymentDetails.Amount,
                _cryptoService.Decrypt(_cardDetails.CardNumberLastFourDigits),
                _cardDetails.ExpiryMonth,
                _cardDetails.ExpiryYear,
                Convert.ToInt32(_cryptoService.Decrypt(_cardDetails.Cvv)),
                _merchant),
                new CancellationToken());

            //Assert
            _paymentRepositoryMock.Verify(_ => _.UpdatePaymentStatusById(_paymentId, BankPaymentStatus.Declined, _merchant), Times.Once);
            Assert.Equal(BankPaymentStatus.Declined, createPaymentResponse.Status);
            Assert.Equal(_paymentId, createPaymentResponse.PaymentId);
        }


        [Fact]
        public async Task CreatePaymentHandler_InvalidMerchant_ThrowsMerchantNotFoundException()
        {
            //Arrange
            var invalidMerchant = _merchant with { MerchantId = Guid.NewGuid() };
            _bankSimulatorMock
                .Setup(_ => _.PostPayment(_bankCardDetails, _paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Declined);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), It.IsAny<Merchant>()))
                .ReturnsAsync(_paymentId);
            _paymentRepositoryMock
                .Setup(_ => _.GetPaymentById(_paymentId, invalidMerchant))
                .ThrowsAsync(new MerchantNotFoundException("Merchant not found"));

            //Act
            //Assert
            await Assert.ThrowsAsync<MerchantNotFoundException>(async () => await _createPaymentHandler.Handle(new(
                _paymentId,
                _paymentDetails.Currency,
                _paymentDetails.Amount,
                _cryptoService.Decrypt(_cardDetails.CardNumberLastFourDigits),
                _cardDetails.ExpiryMonth,
                _cardDetails.ExpiryYear,
                Convert.ToInt32(_cryptoService.Decrypt(_cardDetails.Cvv)),
                invalidMerchant),
                new CancellationToken()));
        }
    }
}