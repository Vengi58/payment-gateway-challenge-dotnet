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

        private readonly BankCardDetails _cardDetailsFull;
        private readonly CardDetails _cardDetails;
        private readonly Guid _paymentId;
        private readonly PaymentDetails _paymentDetails;
        private readonly Merchant _merchant;
        public CreatePaymentHandlerTests()
        {
            _cryptoService = new RsaCryptoService();
            _bankSimulatorMock = new Mock<IBankSimulator>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _createPaymentHandler = new CreatePaymentHandler(_paymentRepositoryMock.Object, _bankSimulatorMock.Object, _cryptoService);

            _cardDetailsFull = new("2222405343248877", 2025, 4, "123");
            _cardDetails = new(_cryptoService.Encrypt(_cardDetailsFull.CardNumber), _cardDetailsFull.ExpiryYear, _cardDetailsFull.ExpiryMonth, _cryptoService.Encrypt(_cardDetailsFull.Cvv));
            _paymentId = Guid.NewGuid();
            _paymentDetails = new(_paymentId, "GBP", 100);
            _merchant = new(Guid.Parse("47f729c1-c863-4403-ae2e-6e836bf44fee"));
        }

        [Fact]
        public async Task CreatePaymentHandler_AuthorizedPayment_PersistedWitCompleted()
        {
            //Arrange
            //CardDetails cardDetails = new(_cryptoService.Encrypt("2222405343248877"), 2025,4, _cryptoService.Encrypt("123"));
            //PaymentDetails paymentDetails = new(paymentId, "GBP", 100);

            _bankSimulatorMock
                .Setup(_ => _.PostPayment(_cardDetailsFull, _paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Authorized);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), It.IsAny<Merchant>()))
                .ReturnsAsync(_paymentId);
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
        public async Task CreatePaymentHandler_DeclinedPayment_PersistedWithDeclined()
        {
            //Arrange

            _bankSimulatorMock
                .Setup(_ => _.PostPayment(_cardDetailsFull, _paymentDetails))
                .ReturnsAsync(BankPaymentStatus.Declined);
            _paymentRepositoryMock
                .Setup(_ => _.CreatePayment(It.IsAny<CardDetails>(), It.IsAny<PaymentDetails>(), It.IsAny<Merchant>()))
                .ReturnsAsync(_paymentId);
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
        }
    }
}