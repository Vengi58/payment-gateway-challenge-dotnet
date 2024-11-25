using System.Security.Cryptography;
using System.Text;

namespace PaymentGateway.Services.Encryption
{
    public class RsaCryptoService : ICryptoService
    {
        private RSA rsa;

        public RsaCryptoService()
        {
            rsa = RSA.Create();
        }

        public string Decrypt(byte[] data)
        {
            var decryptedBytes = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public byte[] Encrypt(string data)
        {
            return rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        }
    }
}
