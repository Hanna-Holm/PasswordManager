
using System.Security.Cryptography;

namespace PasswordManager
{
    internal class Client
    {
        private string _path;
        private readonly int _lengthOfKey = 16;
        public byte[] SecretKeyAsBytes;
        public string MasterPassword;
        public string SecretKey;

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

        public byte[] GetVaultKey(string masterPassword)
        {
            Rfc2898DeriveBytes authentication = new Rfc2898DeriveBytes(masterPassword, SecretKeyAsBytes, 10000, HashAlgorithmName.SHA256);
            return authentication.GetBytes(16);
        }

        public bool CanLoginToServer(string serverPath)
        {
            try
            {
                Server server = new Server(serverPath);
                server.SetIV();
                byte[] encryptedAccounts = server.GetEncryptedAccounts();
                byte[] vaultKey = GetVaultKey();
                string decryptedAccounts = server.Decrypt(encryptedAccounts, vaultKey);
                if (decryptedAccounts == null)
                {
                    return false;
                }
            }
            catch
            {
                Console.WriteLine("Error.");
                return false;
            }

            return true;
        }
    }
}
