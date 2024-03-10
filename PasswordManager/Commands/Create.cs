using PasswordManager.VaultHandler;

namespace PasswordManager.Commands
{
    internal class Create : ICommand
    {

        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 3);
            if (!isArgumentLengthValid)
            {
                return;
            }

            string clientPath = args[1];
            Client client = new Client(clientPath);
            bool shouldAskForSecretKey = true;
            client.Setup(shouldAskForSecretKey);
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

            string newClientPath = args[1];
            CreateNewClient(newClientPath, client.SecretKey);
        }

        private void CreateNewClient(string newClientPath, string secretKey)
        {
            FileHandler fileHandler = new FileHandler();
            fileHandler.WriteToJson(newClientPath, "secret", secretKey);
            Console.WriteLine($"Successfully logged in to server and created new client: {newClientPath}");
        }
    }
}
