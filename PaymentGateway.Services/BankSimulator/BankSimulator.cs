using System.Net.Http.Json;

using PaymentGateway.Api.Services.BankSimulator;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Services.BankSimulator
{
    public class BankSimulator : IBankSimulator
    {
        readonly HttpClient _httpClient = new();

        public async Task<BankPaymentStatus> PostPayment(CardDetails cardDetails, PaymentDetails paymentDetails)
        {
            var zeroPrefix = cardDetails.ExpiryMonth < 10 ? "0" : string.Empty;
            PostBankPaymentRequest request = new(
                cardDetails.CardNumber,
                $"{zeroPrefix}{cardDetails.ExpiryMonth}/{cardDetails.ExpiryYear}",
                paymentDetails.Currency,
                paymentDetails.Amount,
                cardDetails.Cvv.ToString());
            using HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
                "http://localhost:8080/payments",
                request);

            var postPaymentResponse = await httpResponseMessage.Content.ReadFromJsonAsync<PostBankPaymentResponse>();
            return postPaymentResponse == null ? BankPaymentStatus.Declined : postPaymentResponse.authorized ? BankPaymentStatus.Authorized : BankPaymentStatus.Declined;
        }
    }
}
