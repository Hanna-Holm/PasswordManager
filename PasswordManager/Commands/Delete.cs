using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Delete : ICommand
    {
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

            string masterPassword = client.PromptUser("master password");
            byte[] vaultKey = client.GetVaultKey(masterPassword);

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.SetIV();
            byte[] encryptedAccounts = server.GetEncryptedAccounts();
            string decryptedAccounts = server.Decrypt(encryptedAccounts, vaultKey);
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

            server.CreateVault(decryptedUsernamesAndPasswords);
            encryptedAccounts = server.Encrypt(vaultKey);
            File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccounts));
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
