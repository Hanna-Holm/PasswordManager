using PasswordManager.VaultHandler;
using System.Text.Json;

namespace PasswordManager.Commands
{
    internal class Get : ICommand
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
            bool shouldAskForSecretKey = false;
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

            try
            {
                Dictionary<string, string> usernamesAndPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedAccounts);

                if (args.Length == 4)
                {
                    string username = args[3];
                    string requestedPassword = usernamesAndPasswords[username];
                    Console.WriteLine("The requested password is: ");
                    // The password needs to be in a separate console out for the tests to pass!
                    Console.WriteLine(requestedPassword);
                }
                else
                {
                    PrintKeys(usernamesAndPasswords);
                }
            }
            catch
            {
                Console.WriteLine("");
            }
        }

        private static void PrintKeys(Dictionary<string, string> keyValuePairs)
        {
            foreach (var pair in keyValuePairs)
            {
                Console.WriteLine(pair.Key);
            }
        }
    }
}
