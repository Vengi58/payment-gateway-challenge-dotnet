namespace PaymentGateway.Api.Services.BankSimulator
{
    internal record PostBankPaymentResponse(bool authorized, string authorization_code);
}
