
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Client
    {
        private string _path;
        private readonly int _lengthOfKey = 16;
        public byte[] SecretKeyAsBytes;

        public Client(string path)
        {
            _path = path;
        }

        public string PromptUser(string prompt)
        {
            string input = "";

            while (input == "")
            {
                Console.WriteLine($"Enter the {prompt}: ");
                input = Console.ReadLine();
                if (input == "")
                {
                    Console.WriteLine($"The {prompt} cannot be empty.");
                }
            }

            return input;
        }

        public void Initialize()
        {
            GenerateSecretKey();
            string secretKey = GetSecretKeyAsText();
            FileHandler fileHandler = new FileHandler();
            fileHandler.WriteToJson(_path, "secret", secretKey);
        }

        public void GenerateSecretKey()
        {
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                SecretKeyAsBytes = new byte[_lengthOfKey];
                generator.GetBytes(SecretKeyAsBytes);
            }
        }

        public string GetSecretKeyAsText()
        {
            return Convert.ToBase64String(SecretKeyAsBytes);
        }

        public void ReadAndSetSecretKey()
        {
            FileHandler fileHandler = new FileHandler();
            try
            {
                string secretKeyAsText = fileHandler.ReadValueFromJson(_path, "secret");
                SecretKeyAsBytes = Convert.FromBase64String(secretKeyAsText);
            }
            catch
            {
                Console.WriteLine("Could not read from file.");
            }
        }

        public void SetSecretKey(string secretKey)
        {
            try
            {
                SecretKeyAsBytes = Convert.FromBase64String(secretKey);
            }
            catch
            {
                Console.WriteLine("Something went wrong.");
                return;
            }
        }

        public byte[] GetVaultKey(string masterPassword)
        {
            Rfc2898DeriveBytes authentication = new Rfc2898DeriveBytes(masterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
            return authentication.GetBytes(16);
        }
    }
}
