namespace PaymentGateway.Application.Exceptions
{
    public class MerchantNotFoundException : Exception 
    {
        public MerchantNotFoundException()
        {
        }

        public MerchantNotFoundException(string message)
            : base(message)
        {
        }
    }
}
