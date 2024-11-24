namespace PaymentGateway.Api.Services
{
    public interface ICryptoService
    {
        public byte[] Encrypt(string data);
        public string Decrypt(byte[] data);
    }
}
