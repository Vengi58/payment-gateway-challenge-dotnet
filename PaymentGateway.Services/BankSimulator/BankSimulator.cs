using System.Net.Http.Json;

using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Application.Services.BankSimulator.Models;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Models;

namespace PaymentGateway.Services.BankSimulator
{
    public class BankSimulator : IBankSimulator
    {
        readonly HttpClient _httpClient = new();

        public async Task<BankPaymentStatus> PostPayment(BankCardDetails cardDetails, PaymentDetails paymentDetails)
        {
            var zeroPrefix = cardDetails.ExpiryMonth < 10 ? "0" : string.Empty;
            PostBankPaymentRequest request = new(
                cardDetails.CardNumber,
                $"{zeroPrefix}{cardDetails.ExpiryMonth}/{cardDetails.ExpiryYear}",
                paymentDetails.Currency,
                paymentDetails.Amount,
                cardDetails.Cvv);
            using HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
                "http://localhost:8080/payments",
                request);

            var postPaymentResponse = await httpResponseMessage.Content.ReadFromJsonAsync<PostBankPaymentResponse>();
            return postPaymentResponse == null ? BankPaymentStatus.Declined : postPaymentResponse.authorized ? BankPaymentStatus.Authorized : BankPaymentStatus.Declined;
        }
    }
}
