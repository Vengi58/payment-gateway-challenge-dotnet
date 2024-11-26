using System.Net;
using System.Net.Http.Json;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using PaymentGateway.Api.Controllers;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Behaviors;
using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Repository;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;
using PaymentGateway.Persistance.Repository;
using PaymentGateway.Services.Encryption;

namespace PaymentGateway.Api.Tests;

public class PaymentsControllerTests
{
    Mock<IBankSimulator> BankSimulatorMock;
    private readonly ICryptoService _cryptoService;
    HttpClient client;
    private readonly PostPaymentRequest _request;
    private readonly Guid _merchantGuid = Guid.Parse("47f729c1-c863-4403-ae2e-6e836bf44fee");

    public PaymentsControllerTests()
    {
        BankSimulatorMock = new Mock<IBankSimulator>();
        _cryptoService = new RsaCryptoService();

        var webApplicationFactory = new WebApplicationFactory<PaymentsController>();
        client = webApplicationFactory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services => ((ServiceCollection)services)
                .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Application.AssemblyReference.Assembly))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddValidatorsFromAssembly(Application.AssemblyReference.Assembly)
                .AddSingleton<IPaymentRepository, PaymentsRepository>()
                .AddSingleton<ICryptoService>(_cryptoService)
                .AddSingleton<IBankSimulator>(BankSimulatorMock.Object)))
            .CreateClient();

        client.DefaultRequestHeaders.Add("merchant-id", _merchantGuid.ToString());

        _request = new()
        {
            Amount = 100,
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Cvv = 123
        };
    }

    [Fact]
    public async Task PostPayments_ValidPostPaymentRequest_CreatesPaymentSuccessfully()
    {
        //Arrange
        BankCardDetails cardDetailsFull = new(_request.CardNumber, _request.ExpiryYear, _request.ExpiryMonth, _request.Cvv.ToString());
        CardDetails cardDetails = new(_cryptoService.Encrypt(_request.CardNumber), _request.ExpiryYear, _request.ExpiryMonth, _cryptoService.Encrypt(_request.Cvv));
        BankSimulatorMock.Setup(_ => _.PostPayment(cardDetailsFull, It.IsAny<PaymentDetails>())).ReturnsAsync(BankPaymentStatus.Authorized);
        // Act

        client.DefaultRequestHeaders.Add("idempotency-key", "95a1ddd5-b05f-4079-a737-1bb871600f39");
        var response = await client.PostAsJsonAsync($"/api/Payments", _request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(_request.Currency, paymentResponse!.Currency);
        Assert.Equal(_request.Amount, paymentResponse!.Amount);
        Assert.Equal(_request.ExpiryYear, paymentResponse!.ExpiryYear);
        Assert.Equal(_request.ExpiryMonth, paymentResponse!.ExpiryMonth);
        Assert.NotEmpty(paymentResponse!.CardNumberLastFourDigits);
        Assert.Equal(4, paymentResponse!.CardNumberLastFourDigits.Length);
        Assert.Equal(BankPaymentStatus.Authorized, paymentResponse!.Status);
    }

    [Fact]
    public async Task PostPayments_ValidPostPaymentRequestWithIdempotencyKeyDefined_CreatesPaymentSuccessfully()
    {
        //Arrange
        string idKey = "95a1ddd5-b05f-4079-a737-1bb871600f39";
        Guid.TryParse(idKey, out Guid idempotencyKeyGuid);

        BankCardDetails cardDetailsFull = new(_request.CardNumber, _request.ExpiryYear, _request.ExpiryMonth, _request.Cvv.ToString());
        CardDetails cardDetails = new(_cryptoService.Encrypt(_request.CardNumber), _request.ExpiryYear, _request.ExpiryMonth, _cryptoService.Encrypt(_request.Cvv));
        BankSimulatorMock.Setup(_ => _.PostPayment(cardDetailsFull, It.IsAny<PaymentDetails>())).ReturnsAsync(BankPaymentStatus.Authorized);
        client.DefaultRequestHeaders.Add("idempotency-key", idKey);

        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", _request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(idempotencyKeyGuid, paymentResponse!.Id);
        Assert.Equal(_request.Currency, paymentResponse!.Currency);
        Assert.Equal(_request.Amount, paymentResponse!.Amount);
        Assert.Equal(_request.ExpiryYear, paymentResponse!.ExpiryYear);
        Assert.Equal(_request.ExpiryMonth, paymentResponse!.ExpiryMonth);
        Assert.NotEmpty(paymentResponse!.CardNumberLastFourDigits);
        Assert.Equal(4, paymentResponse!.CardNumberLastFourDigits.Length);
        Assert.Equal(BankPaymentStatus.Authorized, paymentResponse!.Status);
    }

    [Fact]
    public async Task PostPayments_AnuauthorizedCardNumber_CreatesPaymentDeclinedByBank()
    {
        //Arrange
        PostPaymentRequest request = new()
        {
            Amount = 100,
            CardNumber = "2222000000000000",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Cvv = 123
        };

        BankCardDetails cardDetailsFull = new(request.CardNumber, request.ExpiryYear, request.ExpiryMonth, request.Cvv.ToString());
        CardDetails cardDetails = new(_cryptoService.Encrypt(request.CardNumber), request.ExpiryYear, request.ExpiryMonth, _cryptoService.Encrypt(request.Cvv));
        BankSimulatorMock.Setup(_ => _.PostPayment(cardDetailsFull, It.IsAny<PaymentDetails>())).ReturnsAsync(BankPaymentStatus.Declined);
        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal(request.Currency, paymentResponse!.Currency);
        Assert.Equal(request.Amount, paymentResponse!.Amount);
        Assert.Equal(request.ExpiryYear, paymentResponse!.ExpiryYear);
        Assert.Equal(request.ExpiryMonth, paymentResponse!.ExpiryMonth);
        Assert.NotEmpty(paymentResponse!.CardNumberLastFourDigits);
        Assert.Equal(4, paymentResponse!.CardNumberLastFourDigits.Length);
        Assert.Equal(BankPaymentStatus.Declined, paymentResponse!.Status);
    }

    [Fact]
    public async Task PostPayments_InvalidCardNumber_CreatesPaymentDeclinedByBank()
    {
        //Arrange
        PostPaymentRequest request = new()
        {
            Amount = 100,
            CardNumber = "222200",
            ExpiryMonth = 4,
            ExpiryYear = 2025,
            Currency = "GBP",
            Cvv = 123
        };

        BankCardDetails cardDetailsFull = new(request.CardNumber, request.ExpiryYear, request.ExpiryMonth, request.Cvv.ToString());
        CardDetails cardDetails = new(_cryptoService.Encrypt(request.CardNumber), request.ExpiryYear, request.ExpiryMonth, _cryptoService.Encrypt(request.Cvv));
        BankSimulatorMock.Setup(_ => _.PostPayment(cardDetailsFull, It.IsAny<PaymentDetails>())).ReturnsAsync(BankPaymentStatus.Declined);
        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        // Assert
        Assert.Equal(HttpStatusCode.UnprocessableContent, response.StatusCode);
        Assert.NotNull(paymentResponse);
        Assert.Equal("ValidationFailure", paymentResponse!.Type);
    }

    [Fact]
    public async Task GetPayments_ForExistingPayment_ReturnsPaymentSuccessfully()
    {
        //Arrange
        BankCardDetails cardDetailsFull = new(_request.CardNumber, _request.ExpiryYear, _request.ExpiryMonth, _request.Cvv.ToString());
        CardDetails cardDetails = new(_cryptoService.Encrypt(_request.CardNumber), _request.ExpiryYear, _request.ExpiryMonth, _cryptoService.Encrypt(_request.Cvv));
        BankSimulatorMock.Setup(_ => _.PostPayment(cardDetailsFull, It.IsAny<PaymentDetails>())).ReturnsAsync(BankPaymentStatus.Authorized);
        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", _request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();


        response = await client.GetAsync($"/api/Payments/{paymentResponse!.Id}");
        var getPaymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(_request.Amount, getPaymentResponse!.Amount);
    }

    [Fact]
    public async Task GetPayments_ForNonExistingPayment_Returns404NotFound()
    {
        //Arrange
        BankCardDetails cardDetailsFull = new(_request.CardNumber, _request.ExpiryYear, _request.ExpiryMonth, _request.Cvv.ToString());
        CardDetails cardDetails = new(_cryptoService.Encrypt(_request.CardNumber), _request.ExpiryYear, _request.ExpiryMonth, _cryptoService.Encrypt(_request.Cvv));
        BankSimulatorMock.Setup(_ => _.PostPayment(cardDetailsFull, It.IsAny<PaymentDetails>())).ReturnsAsync(BankPaymentStatus.Authorized);
        // Act
        var response = await client.PostAsJsonAsync($"/api/Payments", _request);
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();


        response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        var getPaymentResponse = await response.Content.ReadFromJsonAsync<GetPaymentResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPayments_InvalidPaymentId_Returns404INotFound()
    {
        // Arrange
        
        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


    [Fact]
    public async Task GetPayments_MissingMerchantIdHeaderParameter_Returns400BadRequest()
    {
        // Arrange
        client.DefaultRequestHeaders.Remove("merchant-id");

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPayments_InvalidMerchantIdHeaderParameter_Returns404NotFound()
    {
        // Arrange
        client.DefaultRequestHeaders.Remove("merchant-id");
        client.DefaultRequestHeaders.Add("merchant-id", "95a1ddd5-0000-4079-a737-1bb871600f39");

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}