
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

        public void SetSecretKey()
        {
            FileHandler fileHandler = new FileHandler();
            string secretKeyAsString = fileHandler.ReadValueFromJson(_path, "secret");
            SecretKeyAsBytes = Convert.FromBase64String(secretKeyAsString);
        }

        public Rfc2898DeriveBytes DeriveVaultKey()
        {
            Console.WriteLine("Enter your master password: ");
            return new Rfc2898DeriveBytes(Console.ReadLine(), SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
        }
    }
}
