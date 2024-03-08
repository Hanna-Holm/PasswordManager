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
            server.Initialize();

            byte[] encryptedAccountsAsBytes = LockVault(client, server);
            File.WriteAllText(serverPath, server.FormatServerToText(encryptedAccountsAsBytes));
            Console.WriteLine("Successfully initialized client and server.");
        }

        private byte[] LockVault(Client client, Server server) 
        {
            client.MasterPassword = _communicator.PromptUserFor("master password");
            byte[] vaultKey = client.GetVaultKey();
            return server.Encrypt(vaultKey);
        }
    }
}
