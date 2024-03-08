namespace PasswordManager.Commands
{
    internal class Init : ICommand
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
            client.Initialize();

            string serverPath = args[2];
            Server server = new Server(serverPath);
            server.Initialize();

            string masterPassword = client.PromptUser("master password");
            byte[] vaultKey = client.GetVaultKey(masterPassword);
            byte[] encryptedAccountsAsBytes = server.Encrypt(vaultKey);
            File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccountsAsBytes));
            Console.WriteLine("Successfully initialized client and server.");
        }
    }
}
