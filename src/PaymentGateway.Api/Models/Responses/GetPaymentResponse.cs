using System.Text.Json.Serialization;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Api.Models.Responses;

public class GetPaymentResponse
{
    public Guid Id { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BankPaymentStatus Status { get; set; }
    public string CardNumberLastFourDigits { get; set; }
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Currency { get; set; }
    public int Amount { get; set; }
}