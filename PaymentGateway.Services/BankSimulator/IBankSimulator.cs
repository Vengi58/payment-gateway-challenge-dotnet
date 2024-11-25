namespace PaymentGateway.Api.Services.BankSimulator
{
    public interface IBankSimulator
    {
        public Task<PostBankPaymentResponse> PostPayment(PostBankPaymentRequest request); 
    }
}
