namespace PaymentGateway.Application.Services.BankSimulator.Models
{
    public record PostBankPaymentResponse(bool authorized, string authorization_code);
}
