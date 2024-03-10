using PasswordManager.VaultHandler;
using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Delete : ICommand
    {
        private UserCommunicator _communicator = new UserCommunicator();

        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 4);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            client.ReadAndSetSecretKey();
            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            client.MasterPassword = _communicator.PromptUserFor("master password");
            byte[] vaultKey = client.GetVaultKey();

            string serverPath = args[2];
            Server server = new Server(serverPath);
            FileHandler fileHandler = new FileHandler();
            byte[] encryptedAccounts = server.GetEncryptedAccounts();
            string decryptedAccounts = VaultDecryptor.Decrypt(vaultKey, server.IV, encryptedAccounts);
            if (decryptedAccounts == null)
            {
                return;
            }

            Dictionary<string, string> decryptedUsernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccounts);

            string username = args[3];
            if (!CanDeleteAccount(decryptedUsernamesAndPasswords, username))
            {
                return;
            }

            string accounts = JsonSerializer.Serialize(decryptedUsernamesAndPasswords);
            byte[] encryptedAccountsAsBytes = VaultEncryptor.Encrypt(vaultKey, server.IV, accounts);
            File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccountsAsBytes));
        }

        private static bool CanDeleteAccount(Dictionary<string, string> accounts, string username)
        {
            if (accounts.ContainsKey(username))
            {
                accounts.Remove(username);
                Console.WriteLine($"Successfully removed your account {username}");
                return true;
            }
            else
            {
                Console.WriteLine("Account does not exist");
                return false;
            }
        }
    }
}
