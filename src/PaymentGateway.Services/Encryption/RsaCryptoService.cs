using System.Security.Cryptography;
using System.Text;

using PaymentGateway.Application.Encryption;

namespace PaymentGateway.Services.Encryption
{
    public class RsaCryptoService : ICryptoService
    {
        private readonly RSA _rsa;

        public RsaCryptoService()
        {
            _rsa = RSA.Create();
        }

        public string Decrypt(byte[] data)
        {
            var decryptedBytes = _rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public byte[] Encrypt(string data)
        {
            return _rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        }

        public byte[] Encrypt(int data)
        {
            return Encrypt(data.ToString());
        }
    }
}
