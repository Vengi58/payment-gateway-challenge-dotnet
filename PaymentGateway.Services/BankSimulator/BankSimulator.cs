using System.Net.Http.Json;

using PaymentGateway.Api.Services.BankSimulator;

namespace PaymentGateway.Services.BankSimulator
{
    public class BankSimulator : IBankSimulator
    {
        readonly HttpClient _httpClient = new();

        public async Task<PostBankPaymentResponse> PostPayment(PostBankPaymentRequest request)
        {
            using HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
                "http://localhost:8080/payments",
                request);

            var postPaymentResponse = await httpResponseMessage.Content.ReadFromJsonAsync<PostBankPaymentResponse>();
            return postPaymentResponse;
        }
    }
}
