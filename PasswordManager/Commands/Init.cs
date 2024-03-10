using PasswordManager.VaultHandler;
using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Init : ICommand
    {
        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 3);
            if (!isArgumentLengthValid)
                return;

            VaultDecryptor.LoginToServer(args[1], args[2], CommandType.Init);

            string accounts = JsonSerializer.Serialize(new Dictionary<string, string>());
            VaultEncryptor.LockVault(accounts, VaultDecryptor.ServerInstance);

            Console.WriteLine("Successfully initialized client and server.");
        }
    }
}
