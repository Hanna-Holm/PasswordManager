
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Client
    {
        private string _path;
        private readonly int _lengthOfKey = 16;
        public string MasterPassword;
        public byte[] SecretKeyAsBytes;
        public string SecretKey;
        UserCommunicator _communicator = new UserCommunicator();

        public Client(string path)
        {
            _path = path;
        }

        public void TrySetSecretFromFile()
        {
            if (File.Exists(_path))
            {
                FileHandler fileHandler = new FileHandler();
                SecretKey = fileHandler.ReadValueFromJson(_path, "secret");
                SecretKeyAsBytes = Convert.FromBase64String(SecretKey);
            }
        }

        public void Initialize()
        {
            GenerateSecretKey();
            string secretKey = Convert.ToBase64String(SecretKeyAsBytes);
            FileHandler fileHandler = new FileHandler();
            fileHandler.WriteToJson(_path, "secret", secretKey);
        }

        private void GenerateSecretKey()
        {
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                SecretKeyAsBytes = new byte[_lengthOfKey];
                generator.GetBytes(SecretKeyAsBytes);
            }
        }

        public void SetSecretKeyFromPrompt()
        {
            SecretKey = _communicator.PromptUserFor("secret key");
            SetSecretKey();
        }

        public void SetSecretKey()
        {
            try
            {
                SecretKeyAsBytes = Convert.FromBase64String(SecretKey);
            }
            catch
            {
                Console.WriteLine("Something went wrong.");
                return;
            }
        }

        public byte[] GetVaultKey()
        {
            Rfc2898DeriveBytes authentication = new Rfc2898DeriveBytes(MasterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
            return authentication.GetBytes(16);
        }
    }
}
