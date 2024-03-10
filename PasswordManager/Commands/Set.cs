using PasswordManager.VaultHandler;
using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Set : ICommand
    {
        private UserCommunicator _communicator = new UserCommunicator();
        private string _username;
        private bool _shouldAskForSecretKey = false;

        public void Run(string[] args)
        {
            Validator validator = new Validator();

            bool isArgumentLengthValid = validator.ValidateArgumentsLength(args, 4);
            bool isUsernameProvided = validator.CheckIfUsernameIsProvided(args);
            if (!isArgumentLengthValid || !isUsernameProvided)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            client.Setup(_shouldAskForSecretKey);
            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            string serverPath = args[2];
            Server server = new Server(serverPath);
            byte[] encryptedAccounts = server.GetEncryptedAccounts();
            byte[] vaultKey = client.GetVaultKey();
            string decryptedAccounts = VaultDecryptor.Decrypt(vaultKey, server.IV, encryptedAccounts);
            
            if (decryptedAccounts == null)
            {
                return;
            }

            try
            {
                _username = args[3];

                string password = SetPassword(args);
                
                Dictionary<string, string> decryptedUsernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccounts);

                if (!decryptedUsernamesAndPasswords.TryAdd(_username, password))
                {
                    decryptedUsernamesAndPasswords[_username] = password;
                }

                Console.WriteLine($"Successfully stored password: {password} for {_username}");
                //server.Accounts = decryptedUsernamesAndPasswords;
                decryptedAccounts = JsonSerializer.Serialize(decryptedUsernamesAndPasswords);
                encryptedAccounts = VaultEncryptor.Encrypt(vaultKey, server.IV, decryptedAccounts);
                File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccounts));
            }
            catch
            {
                Console.WriteLine("Something went wrong");
            }
        }

        private string SetPassword(string[] args)
        {
            if (args.Length == 5 && (args[4] == "-g" || args[4] == "--generate"))
            {
                return new PasswordGenerator().Generate();
            }
            else
            {
                return _communicator.PromptUserFor($"password to store for {_username}");
            }
        }
    }
}
