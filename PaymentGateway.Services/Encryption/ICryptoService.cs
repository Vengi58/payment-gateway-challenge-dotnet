namespace PaymentGateway.Services.Encryption
{
    public interface ICryptoService
    {
        public byte[] Encrypt(string data);
        public string Decrypt(byte[] data);
    }
}
