namespace PaymentGateway.Application.Encryption
{
    public interface ICryptoService
    {
        public byte[] Encrypt(string data);
        public byte[] Encrypt(int data);
        public string Decrypt(byte[] data);
    }
}
