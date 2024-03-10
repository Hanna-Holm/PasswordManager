
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

        public Client(string path)
        {
            _path = path;
        }

        public void Initialize()
        {
            GenerateSecretKey();
            string secretKey = Convert.ToBase64String(SecretKeyAsBytes);
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

        public void Setup(bool shouldAskForSecretKey)
        {
            UserCommunicator communicator = new UserCommunicator();
            MasterPassword = communicator.PromptUserFor("master password");

            if (shouldAskForSecretKey)
            {
                SecretKey = communicator.PromptUserFor("secret key");
                SetSecretKey();
            }
            else
            {
                ReadAndSetSecretKey();
            }
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

        public void ReadAndSetSecretKey()
        {
            FileHandler fileHandler = new FileHandler();
            try
            {
                SecretKey = fileHandler.ReadValueFromJson(_path, "secret");
                SecretKeyAsBytes = Convert.FromBase64String(SecretKey);
            }
            catch
            {
                Console.WriteLine("Could not read from file.");
            }
        }

        public byte[] GetVaultKey()
        {
            Rfc2898DeriveBytes authentication = new Rfc2898DeriveBytes(MasterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
            return authentication.GetBytes(16);
        }
    }
}
