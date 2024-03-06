
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Client
    {
        private string _path;
        private int _lengthOfKey = 16;
        public byte[] SecretKeyAsBytes;

        public Client(string path)
        {
            _path = path;
        }

        public string GenerateSecretKey()
        {
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                SecretKeyAsBytes = new byte[_lengthOfKey];
                generator.GetBytes(SecretKeyAsBytes);
            }

            return Convert.ToBase64String(SecretKeyAsBytes);
        }

        public void SetSecretKey(string secretKeyAsString)
        {
            SecretKeyAsBytes = Convert.FromBase64String(secretKeyAsString);
        }

        public Rfc2898DeriveBytes Authenticate(string masterPassword)
        {
            return new Rfc2898DeriveBytes(masterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
        }
    }
}
