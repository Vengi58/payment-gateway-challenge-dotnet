namespace PaymentGateway.Api.Services.BankSimulator
{
    public record PostBankPaymentResponse(bool authorized, string authorization_code);
}
