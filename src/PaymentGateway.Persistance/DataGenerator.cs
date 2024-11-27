using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Application.Encryption;
using PaymentGateway.Persistance.Entities;
using PaymentGateway.Persistance.Repository;

namespace PaymentGateway.Persistance
{
    public class DataGenerator
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var cryptoService = serviceProvider.GetRequiredService<ICryptoService>();
            PaymentsDbContext paymentsDbContext = serviceProvider.GetRequiredService<PaymentsDbContext>();

            paymentsDbContext.Merchants.RemoveRange(paymentsDbContext.Merchants);
            paymentsDbContext.Payments.RemoveRange(paymentsDbContext.Payments);
            paymentsDbContext.SaveChanges();

            PaymentEntity paymentEntity = new()
            {
                Amount = 100,
                CardNumberLastFour = cryptoService.Encrypt("1677"),
                Currency = "GBP",
                ExpiryMonth = 11,
                ExpiryYear = 2025,
                PaymentId = Guid.Parse("b38eacca-a23e-4988-bbe6-ffba1ed3958b"),
                PaymentProcessingStatus = PaymentProcessingStatus.FinishedProcessing
            };

            paymentsDbContext.Payments.Add(paymentEntity);

            paymentsDbContext.Merchants.Add(new()
            {
                MerchantId = Guid.Parse("47f729c1-c863-4403-ae2e-6e836bf44fee"),
                Payments = [paymentEntity]
            });
            paymentsDbContext.Merchants.Add(new()
            {
                MerchantId = Guid.Parse("cc6afe80-e0c2-478d-a50a-18d68c918352"),
                Payments = []
            });
            paymentsDbContext.SaveChanges();
        }
    }
}
