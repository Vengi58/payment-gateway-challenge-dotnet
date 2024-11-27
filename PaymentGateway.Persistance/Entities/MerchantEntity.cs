using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Persistance.Entities
{
    public class MerchantEntity
    {
        [Key]
        public Guid MerchantId { get; set; }
        public ICollection<PaymentEntity> Payments { get; set; } = [];

    }
}
