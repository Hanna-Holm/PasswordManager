using PasswordManager.VaultHandler;
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

            VaultDecryptor.LoginToServer(args[1], args[2], false, false);
            if (VaultDecryptor.DecryptedAccounts == null)
            {
                return;
            }

            Dictionary<string, string> decryptedUsernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(VaultDecryptor.DecryptedAccounts);
            string username = args[3];
            if (!CanDeleteAccount(decryptedUsernamesAndPasswords, username))
            {
                return;
            }

            string accounts = JsonSerializer.Serialize(decryptedUsernamesAndPasswords);
            VaultEncryptor.LockVault(accounts, VaultDecryptor.ServerInstance);
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
