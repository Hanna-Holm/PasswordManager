using PasswordManager.VaultHandler;

namespace PasswordManager.Commands
{
    internal class Create : ICommand
    {

        public void Run(string[] args)
        {
            bool isArgumentLengthValid = new Validator().ValidateArgumentsLength(args, 3);
            if (!isArgumentLengthValid)
                return;

            VaultDecryptor.LoginToServer(args[1], args[2], CommandType.Create);
            if (VaultDecryptor.DecryptedAccounts == null)
                return;

            string newClientPath = args[1];
            CreateNewClient(newClientPath, VaultDecryptor.SecretKey);
        }

        private void CreateNewClient(string newClientPath, string secretKey)
        {
            FileHandler fileHandler = new FileHandler();
            fileHandler.WriteToJson(newClientPath, "secret", secretKey);
            Console.WriteLine($"Successfully logged in to server and created new client: {newClientPath}");
        }
    }
}
