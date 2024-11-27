using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentGateway.Persistance.Entities
{
    public class PaymentEntity
    {
        [Key]
        public Guid PaymentId { get; set; }
        public byte[] CardNumberLastFour { get; set; }
        public int ExpiryYear { get; set; }
        public int ExpiryMonth { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
        public PaymentProcessingStatus PaymentProcessingStatus { get; set; }
        public MerchantEntity Merchant { get; set; } = null!;
    }
}
