using PasswordManager.VaultHandler;
using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Init : ICommand
    {
        private UserCommunicator _communicator = new UserCommunicator();

        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 3);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            client.Initialize();

            string serverPath = args[2];
            Server server = new Server(serverPath);

            client.MasterPassword = _communicator.PromptUserFor("master password");
            byte[] vaultKey = client.GetVaultKey();
            string accounts = JsonSerializer.Serialize(new Dictionary<string, string>());
            byte[] encryptedAccountsAsBytes =  VaultEncryptor.Encrypt(vaultKey, server.IV, accounts);

            File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccountsAsBytes));
            Console.WriteLine("Successfully initialized client and server.");
        }
    }
}
