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
            string masterPassword = client.PromptUser("master password");
            string secretKey = client.PromptUser("secret key");
            client.SetSecretKey(secretKey);
            if (client.SecretKeyAsBytes == null)
            {
                return;
            }

            string serverPath = args[2];
            Server server = new Server(serverPath);
            try
            {
                server.SetIV();
                byte[] encryptedAccounts = server.GetEncryptedAccounts();
                byte[] vaultKey = client.GetVaultKey(masterPassword);
                string decryptedAccounts = server.Decrypt(encryptedAccounts, vaultKey);
                if (decryptedAccounts == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            string newClientPath = args[1];
            FileHandler fileHandler = new FileHandler();
            fileHandler.WriteToJson(newClientPath, "secret", secretKey);
            Console.WriteLine($"Successfully logged in to server and created new client: {newClientPath}");
        }
    }
}
