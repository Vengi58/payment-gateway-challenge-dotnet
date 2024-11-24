namespace PaymentGateway.Api.Services.BankSimulator
{
    public record PostBankPaymentRequest(string card_number, string expiry_date, string currency, int amount, string cvv);
}
